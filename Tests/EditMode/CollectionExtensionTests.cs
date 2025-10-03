using System;
using System.Collections.Generic;
using NUnit.Framework;
using Utilities;

public class CollectionExtensionTests
{
    public static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(new List<int> {1, 2, 3, 4, 5}, false).SetName("List<int>(5)");
        yield return new TestCaseData(new List<int>(), true).SetName("Empty List<int>");
        yield return new TestCaseData(null, true).SetName("null List<int>");
        
        yield return new TestCaseData(new int[]{1, 2, 3, 4, 5}, false).SetName("int[5]");
        yield return new TestCaseData(Array.Empty<int>(), true).SetName("Empty int[]");
        yield return new TestCaseData(null, true).SetName("null int[]");
    }
    
    // A Test behaves as an ordinary method
    [TestCaseSource(nameof(TestCases))]
    public void PickRandomElementTest(IEnumerable<int> collection, bool shouldFail)
    {
        switch (collection)
        {
            case int[] array:
                PickRandomArrayElementTest(array, shouldFail);
                break;
            case List<int> list:
                PickRandomListElementTest(list, shouldFail);
                break;
        }
    }
    
        
        // A Test behaves as an ordinary method
    [TestCaseSource(nameof(TestCases))]
    public void ShuffleTest(IEnumerable<int> collection, bool shouldFail)
    {
        switch (collection)
        {
            case int[] array:
                ShuffleArrayTest(array, shouldFail);
                break;
            case List<int> list:
                ShuffleListTest(list, shouldFail);
                break;
        }
    }

    #region Expected To Pass

    //================================================================================================================//
    
    // A Test behaves as an ordinary method
    public void PickRandomListElementTest(List<int> list, bool shouldFail)
    {
        if (shouldFail)
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var element = list.PickRandomElement();
            });
            
            return;
        }
        
        var element = list.PickRandomElement();
        Assert.IsNotNull(element);
    }

    
    // A Test behaves as an ordinary method
    public void ShuffleListTest(List<int> list, bool shouldFail)
    {
        if (shouldFail)
        {
            Assert.Throws<InvalidOperationException>(list.Shuffle);
            return;
        }
        
        list.Shuffle();
        Assert.IsNotNull(list);
        Assert.IsNotEmpty(list);
    }
    
    //================================================================================================================//

    // A Test behaves as an ordinary method
    public void PickRandomArrayElementTest(int[] array, bool shouldFail)
    {
        if (shouldFail)
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var element = array.PickRandomElement();
            });
            
            return;
        }
        
        var element = array.PickRandomElement();
        Assert.IsNotNull(element);
    }

    
    // A Test behaves as an ordinary method
    public void ShuffleArrayTest(int[] array, bool shouldFail)
    {
        if (shouldFail)
        {
            Assert.Throws<InvalidOperationException>(array.Shuffle);
            return;
        }
        
        array.Shuffle();
        Assert.IsNotNull(array);
        Assert.IsNotEmpty(array);
    }

    #endregion

}
