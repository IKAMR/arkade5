using Arkivverket.Arkade.Core.Testing.Siard;
using FluentAssertions;
using Xunit;

namespace Arkivverket.Arkade.Core.Tests.Testing.Siard;

public class SiardValidatorTest
{
    [Fact]
    public void RecognizesErrorFromJavaRuntimeOlderThanTheValidatorLibrary()
    {
        const string javaRuntimeError =
            "\tjava.lang.UnsupportedClassVersionError: com/databasepreservation/Main has been compiled by a more " +
            "recent version of the Java Runtime (class file version 65.0), this version of the Java Runtime only " +
            "recognizes class file versions up to 52.0";

        SiardValidator.IndicatesUnsupportedJavaRuntime(javaRuntimeError).Should().BeTrue();
    }

    [Fact]
    public void AcceptsValidationRunOutputAsIs()
    {
        SiardValidator.IndicatesUnsupportedJavaRuntime(null).Should().BeFalse();
        SiardValidator.IndicatesUnsupportedJavaRuntime(string.Empty).Should().BeFalse();
        SiardValidator.IndicatesUnsupportedJavaRuntime(
            "The validator only supports: SIARD 2.1 or 2.2 versions").Should().BeFalse();
    }
}
