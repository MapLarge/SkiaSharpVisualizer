namespace SkiaSharpVisualizer;

using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Remote user control to visualize the <see cref="SkiaSharpVisualizerDataContext"/> value.
/// </summary>
internal partial class SkiaSharpVisualizerControl : RemoteUserControl {

	public SkiaSharpVisualizerDataContext? TypedContext => this.DataContext as SkiaSharpVisualizerDataContext;

	public SkiaSharpVisualizerControl(VisualizerTarget visualizerTarget)
		: base(dataContext: new SkiaSharpVisualizerDataContext(visualizerTarget)) {
	}

	public override Task<string> GetXamlAsync(CancellationToken cancellationToken) {
		return base.GetXamlAsync(cancellationToken);
	}
	public override Task ControlLoadedAsync(CancellationToken cancellationToken) {
		return base.ControlLoadedAsync(cancellationToken);
	}

	protected override void Dispose(bool disposing) {
		if (disposing) {
			TypedContext?.Dispose();
		}
		base.Dispose(disposing);
	}

}
