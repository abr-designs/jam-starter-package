// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Holds the parse state for a single <see cref="TMP_Text"/> and runs its effect spans each frame.
    /// Caches the link spans and a snapshot of the un-animated vertices so per-frame work stays pure math.
    /// The snapshot is kept in flat buffers that are reused across refreshes and indexed per material.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public class AnimatedText : IDisposable
    {
        //Fields
        //================================================================================================================//

        #region Fields

        private readonly TMP_Text m_textComponent;
        private readonly List<EffectRange> m_effectRangeList;
        private readonly TextMeshProUGUI m_uguiText;
        private readonly CanvasRenderer m_canvasRenderer;
        private readonly Renderer m_meshRenderer;

        private Vector3[] m_originalVertices;
        private Color32[] m_originalColors;
        private int[] m_meshVertexStart;
        private int m_meshCount;

        private bool m_isRefreshing;

        #endregion //Fields

        //Properties
        //================================================================================================================//

        #region Properties

        public TMP_Text TextComponent => m_textComponent;
        public bool IsValid => m_textComponent != null;
        public bool HasSpans => m_effectRangeList.Count > 0;

        #endregion //Properties

        //Constructors
        //================================================================================================================//

        #region Constructors

        public AnimatedText(TMP_Text textComponent)
        {
            m_textComponent = textComponent;
            m_effectRangeList = new List<EffectRange>();

            m_uguiText = textComponent as TextMeshProUGUI;
            if (m_uguiText != null)
                m_canvasRenderer = m_uguiText.canvasRenderer;
            else
                m_meshRenderer = textComponent.GetComponent<Renderer>();

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            Refresh();
        }

        #endregion //Constructors

        //Public Methods
        //================================================================================================================//

        #region Public Methods

        public void Refresh()
        {
            if (IsValid == false)
                return;

            m_isRefreshing = true;

            m_textComponent.ForceMeshUpdate();
            SnapshotVertices();
            BuildSpans();

            m_isRefreshing = false;
        }

        public void Apply(float time)
        {
            if (IsValid == false || HasSpans == false || IsVisible() == false)
                return;

            var textInfo = m_textComponent.textInfo;

            for (int spanIndex = 0; spanIndex < m_effectRangeList.Count; spanIndex++)
            {
                var span = m_effectRangeList[spanIndex];
                for (int charOffset = 0; charOffset < span.Length; charOffset++)
                {
                    var characterIndex = span.Start + charOffset;
                    if (characterIndex >= textInfo.characterCount)
                        break;

                    var characterInfo = textInfo.characterInfo[characterIndex];
                    if (characterInfo.isVisible == false)
                        continue;

                    ApplyToCharacter(textInfo, characterInfo, span.Effect, charOffset, time);
                }
            }

            m_textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        public void Restore()
        {
            if (IsValid == false || m_originalVertices == null)
                return;

            var textInfo = m_textComponent.textInfo;
            var meshCount = Mathf.Min(m_meshCount, textInfo.meshInfo.Length);

            for (int i = 0; i < meshCount; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                var start = m_meshVertexStart[i];

                Array.Copy(m_originalVertices, start, meshInfo.vertices, 0, meshInfo.vertices.Length);
                Array.Copy(m_originalColors, start, meshInfo.colors32, 0, meshInfo.colors32.Length);
            }

            m_textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        public void Dispose()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        #endregion //Public Methods

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        private void SnapshotVertices()
        {
            var textInfo = m_textComponent.textInfo;
            m_meshCount = textInfo.meshInfo.Length;

            if (m_meshVertexStart == null || m_meshVertexStart.Length < m_meshCount)
                m_meshVertexStart = new int[m_meshCount];

            var totalVertices = 0;
            for (int i = 0; i < m_meshCount; i++)
            {
                m_meshVertexStart[i] = totalVertices;
                totalVertices += textInfo.meshInfo[i].vertices.Length;
            }

            if (m_originalVertices == null || m_originalVertices.Length < totalVertices)
            {
                m_originalVertices = new Vector3[totalVertices];
                m_originalColors = new Color32[totalVertices];
            }

            for (int i = 0; i < m_meshCount; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                var start = m_meshVertexStart[i];

                Array.Copy(meshInfo.vertices, 0, m_originalVertices, start, meshInfo.vertices.Length);
                Array.Copy(meshInfo.colors32, 0, m_originalColors, start, meshInfo.colors32.Length);
            }
        }

        private void BuildSpans()
        {
            m_effectRangeList.Clear();

            var textInfo = m_textComponent.textInfo;
            for (int i = 0; i < textInfo.linkCount; i++)
            {
                var linkInfo = textInfo.linkInfo[i];
                var effect = TextEffectRegistry.Get(linkInfo.GetLinkID());
                if (effect == null)
                    continue;

                m_effectRangeList.Add(new EffectRange
                {
                    Effect = effect,
                    Start = linkInfo.linkTextfirstCharacterIndex,
                    Length = linkInfo.linkTextLength,
                });
            }
        }

        private void ApplyToCharacter(
            TMP_TextInfo textInfo,
            TMP_CharacterInfo characterInfo,
            TextEffect effect,
            int charIndex,
            float time)
        {
            var materialIndex = characterInfo.materialReferenceIndex;
            var sourceBase = m_meshVertexStart[materialIndex] + characterInfo.vertexIndex;
            var vertexIndex = characterInfo.vertexIndex;

            var vertices = textInfo.meshInfo[materialIndex].vertices;
            var colors = textInfo.meshInfo[materialIndex].colors32;

            var mod = CharMod.Identity;
            effect.Apply(ref mod, charIndex, time);

            var center =
                (m_originalVertices[sourceBase] +
                 m_originalVertices[sourceBase + 1] +
                 m_originalVertices[sourceBase + 2] +
                 m_originalVertices[sourceBase + 3]) * 0.25f;

            var rotation = Quaternion.AngleAxis(mod.RotationDeg, Vector3.forward);

            for (int corner = 0; corner < 4; corner++)
            {
                var source = m_originalVertices[sourceBase + corner];
                var offsetFromCenter = (source - center) * mod.Scale;

                vertices[vertexIndex + corner] = center + rotation * offsetFromCenter + mod.Offset;
                colors[vertexIndex + corner] = MultiplyColor(m_originalColors[sourceBase + corner], mod.Color);
            }
        }

        // Cheap, per-frame gate so hidden labels skip the per-character vertex work. Effects are
        // stateless in time, so a paused label resumes seamlessly once it is shown again.
        private bool IsVisible()
        {
            if (!m_textComponent.isActiveAndEnabled)
                return false;

            if (m_uguiText == null) 
                return m_meshRenderer == null || m_meshRenderer.isVisible;
            
            if (m_canvasRenderer != null &&
                (m_canvasRenderer.cull || m_canvasRenderer.GetInheritedAlpha() <= 0.001f))
                return false;

            var canvas = m_uguiText.canvas;
            return canvas != null && canvas.isActiveAndEnabled;

        }

        private void OnTextChanged(UnityEngine.Object changedObject)
        {
            if (m_isRefreshing || changedObject != m_textComponent)
                return;

            Refresh();
        }

        private static Color32 MultiplyColor(Color32 source, Color32 modifier)
        {
            return new Color32(
                (byte)(source.r * modifier.r / 255),
                (byte)(source.g * modifier.g / 255),
                (byte)(source.b * modifier.b / 255),
                (byte)(source.a * modifier.a / 255));
        }

        #endregion //Private Methods
    }
}
