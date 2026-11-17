using System.Collections.Generic;
using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    public class ReactionTests
    {
        ReactionDefinition uap, ledakan;
        List<ReactionDefinition> book;

        [SetUp]
        public void SetUp()
        {
            uap = Make(Element.Fire, Element.Ice);
            ledakan = Make(Element.Fire, Element.Lightning);
            book = new List<ReactionDefinition> { uap, ledakan };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(uap);
            Object.DestroyImmediate(ledakan);
        }

        static ReactionDefinition Make(Element a, Element b)
        {
            var d = ScriptableObject.CreateInstance<ReactionDefinition>();
            d.elementA = a;
            d.elementB = b;
            return d;
        }

        [Test] public void Find_FireIce_ReturnsUap() => Assert.AreSame(uap, ReactionResolver.Find(Element.Fire, Element.Ice, book));
        [Test] public void Find_IsOrderIndependent() => Assert.AreSame(uap, ReactionResolver.Find(Element.Ice, Element.Fire, book));
        [Test] public void Find_FireLightning_ReturnsLedakan() => Assert.AreSame(ledakan, ReactionResolver.Find(Element.Lightning, Element.Fire, book));
        [Test] public void Find_NoRule_ReturnsNull() => Assert.IsNull(ReactionResolver.Find(Element.Ice, Element.Lightning, book));
        [Test] public void Find_SameElement_ReturnsNull() => Assert.IsNull(ReactionResolver.Find(Element.Fire, Element.Fire, book));
        [Test] public void Find_None_ReturnsNull() => Assert.IsNull(ReactionResolver.Find(Element.Fire, Element.None, book));
    }
}
