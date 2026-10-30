using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Upload;

public class UploadRedesignContractTests
{
    [Fact]
    public void Admission_Decision_Accepts_And_Rejects_Without_Exceptions()
    {
        UploadAdmissionDecision.Accept().IsAccepted.ShouldBeTrue();
        var rejection = UploadAdmissionDecision.Reject("blocked", "Policy blocked the file.");
        rejection.IsAccepted.ShouldBeFalse();
        rejection.RejectionCode.ShouldBe("blocked");
        rejection.Message.ShouldBe("Policy blocked the file.");
    }
}
