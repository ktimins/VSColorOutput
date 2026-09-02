using System.ComponentModel.Composition;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using VSColorOutput.Output.Zoom;

namespace Tests
{
    [TestClass]
    public class OutputZoomTextViewCreationListenerTests
    {
        [TestMethod]
        public void HasOutputTextViewCreationListenerAttributes()
        {
            var listenerType = typeof(ZoomEnabler).Assembly.GetType(
                "VSColorOutput.Output.Zoom.OutputZoomTextViewCreationListener");

            listenerType.Should().NotBeNull();
            listenerType.Should().BeDecoratedWith<ExportAttribute>(
                attribute => attribute.ContractType == typeof(IWpfTextViewCreationListener));
            listenerType.Should().BeDecoratedWith<ContentTypeAttribute>(
                attribute => attribute.ContentTypes == "output");
            listenerType.Should().BeDecoratedWith<TextViewRoleAttribute>(
                attribute => attribute.TextViewRoles == PredefinedTextViewRoles.Interactive);
            listenerType.Should().Implement<IWpfTextViewCreationListener>();
        }
    }
}
