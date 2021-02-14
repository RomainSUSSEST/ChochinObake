using System.Collections.Generic;

public class ManagersStates
{
	#region Attributs

	private static List<ManagersStates> Managers = new List<ManagersStates>();

	public bool state;

	#endregion

	#region Constructeur

	public ManagersStates()
	{
		Add(this);
	}

	#endregion

	#region Methods

	public static bool AllManagersReady()
	{
		foreach (ManagersStates m in Managers)
		{
			if (!m.state)
				return false;
		}

		return true;
	}

	public void Destroy()
	{
		Managers.Remove(this);
	}

	#endregion

	#region Tools

	private void Add(ManagersStates m)
	{
		Managers.Add(m);
	}

	#endregion
}