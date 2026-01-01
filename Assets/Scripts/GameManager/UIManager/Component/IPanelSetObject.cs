namespace GameUI
{
    public interface IPanelSetObject : IPanelItem
    {
		public IObjectForPanel Target { get; }
		public bool SetTarget(IObjectForPanel target);
	}
}
