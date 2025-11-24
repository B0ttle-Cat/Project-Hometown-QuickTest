public interface ISectorRendering
{
	ISectorRendering ThisRendering { get; }

}
public partial class SectorObject : ISectorRendering
{
	public ISectorRendering ThisRendering => this;
}