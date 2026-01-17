public interface ISelectVisualizer
{
	public ISelectable ThisSelectable { get; }
	public ISelectVisualizer ThisSelectVisualizer => this;
	void OnPointEnter() { }
	void OnPointExit() { }
	void OnSelect();
	void OnDeselect();
	void OnPointing();
}
									   