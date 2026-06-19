# New Sample — Reference Formats

## package.json entry

Append to the `"samples"` array in `./package.json`:

```json
{
  "displayName": "{DisplayName}",
  "description": "{description}",
  "path": "Samples~/{DirectoryName}"
}
```

**Description with dependencies** (HTML is supported in Package Manager):
```json
"description": "Brief description.<br><u>Dependencies:</u><br>- <a href=\"\">Other Sample Name</a>"
```

Empty description is valid: `"description": ""`

## Documentation markdown template

File: `Documentation~/Samples/{kebab-name}.md`

```markdown
---
title: {DisplayName}
---
# {DisplayName}

{description}

## Overview

<!-- TODO: describe what this sample provides -->

## Usage

<!-- TODO: explain how to set up and use this sample -->
```

Add any extra sections the user requested after `## Usage`:

```markdown
## {ExtraSection}

<!-- TODO -->
```

## README.md entry format

Section header: `## Samples`

Entry format (appended after the last `- ###` line in the Samples section):
```markdown
- ### [{DisplayName}](Documentation~/Samples/{kebab-name}.md)
```

## Existing samples (for reference / conflict checking)

Check `package.json` `"samples"` array for the current list before creating. Directory names must be unique under `Samples~/`.

## Kebab naming rules

| Display Name          | Kebab name              | Directory      |
|-----------------------|-------------------------|----------------|
| VFX Manager           | vfx-manager             | VFXManager     |
| 2D Character Controller | 2d-character-controller | 2DController  |
| Dialog System         | dialog-system           | DialogSystem   |
| Thumbnail Studio      | thumbnail-studio        | ThumbnailStudio|

Strip special characters, lowercase everything, spaces → hyphens for kebab. For directory names, use PascalCase of the display name words (drop articles like "a", "the" if awkward).
