using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Generator.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Generator.Tests.Model;

[TestClass, TestCategory("UnitTest")]
public class MethodTests
{
    private readonly GirLoader.Input.Repository _repository;

    public MethodTests()
    {
        _repository = GenerateTestRepository();
    }

    [TestMethod]
    public void DetectsHiddenMethodFromGirParent()
    {
        var loader = new GirLoader.Loader(_ => null);
        var outputs = loader.Load(new[] { _repository });
        var method = outputs
            .First(repository => repository.Namespace.Name == "Test")
            .Namespace
            .Classes.First(c => c.Name == "B")
            .Methods.First(m => m.Name == "foo");

        Method.HidesMethod(method).Should().BeTrue("this method hides foo from class A");
    }

    [TestMethod]
    public void DetectsHiddenMethodFromSystemObject()
    {
        var loader = new GirLoader.Loader(_ => null);
        var outputs = loader.Load(new[] { _repository });
        var toStringMethod = outputs
            .First(repository => repository.Namespace.Name == "Test")
            .Namespace
            .Classes.First(c => c.Name == "A")
            .Methods.First(m => m.Name == "to_string");

        Method.HidesMethod(toStringMethod).Should().BeTrue("this method hides ToString on System.Object");
    }

    [TestMethod]
    public void NoHiddenMethod()
    {
        var loader = new GirLoader.Loader(_ => null);
        var outputs = loader.Load(new[] { _repository });
        var method = outputs
            .First(repository => repository.Namespace.Name == "Test")
            .Namespace
            .Classes.First(c => c.Name == "A")
            .Methods.First(m => m.Name == "foo");

        Method.HidesMethod(method).Should().BeFalse("this method does not hide any parent class method");
    }

    private static GirLoader.Input.Repository GenerateTestRepository()
    {
        return new GirLoader.Input.Repository
        {
            Namespace = new GirLoader.Input.Namespace
            {
                Name = "Test",
                Version = "1.0",
                Classes = new List<GirLoader.Input.Class>
                {
                    new ()
                    {
                        Name = "A",
                        GetTypeFunction = "test_a_get_type",
                        Methods = new List<GirLoader.Input.Method>
                        {
                            StringMethod("a", "foo"),
                            StringMethod("a", "to_string"),
                        }
                    },
                    new ()
                    {
                        Name = "B",
                        Parent = "A",
                        GetTypeFunction = "test_b_get_type",
                        Methods = new List<GirLoader.Input.Method>
                        {
                            StringMethod("b", "foo"),
                        }
                    }
                }
            }
        };
    }

    private static GirLoader.Input.Method StringMethod(string className, string methodName)
    {
        return new GirLoader.Input.Method
        {
            Name = methodName,
            Identifier = $"test_{className}_{methodName}",
            ReturnValue = new GirLoader.Input.ReturnValue
            {
                TransferOwnership = "full",
                Type = new() { Name = "utf8", CType = "char*" }
            }
        };
    }
}
