using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NUnit.Framework;
using RediSearchSharp.Utils;

namespace RediSearchSharp.Tests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class PropertySelectorExtensionsTests
    {
        public class GetMemberName
        {
            private class ChildType
            {
                public string ChildTypeProperty { get; set; }
            }

            private class TestType
            {
                public string TestTypeProperty { get; set; }
                public ChildType ChildType { get; set; }
            }

            [Test]
            public void Should_throw_when_the_property_selector_is_not_a_member_expression()
            {
                Expression<Func<TestType, string>> testExpression = t => t.ToString();
                Assert.Throws<ArgumentException>(() =>
                {
                    testExpression.GetMemberName();
                });
            }

            [Test]
            public void Should_throw_when_the_expression_is_not_referring_to_a_property_of_the_type()
            {
                Expression<Func<TestType, string>> testExpression = t => t.ChildType.ChildTypeProperty;
                Assert.Throws<ArgumentException>(() =>
                {
                    testExpression.GetMemberName();
                });
            }

            [Test]
            public void Should_return_the_property_name()
            {
                Expression<Func<TestType, string>> testExpression = t => t.TestTypeProperty;

                string memberName = testExpression.GetMemberName();

                Assert.That(memberName, Is.EqualTo("TestTypeProperty"));
            }
        }
    }
}