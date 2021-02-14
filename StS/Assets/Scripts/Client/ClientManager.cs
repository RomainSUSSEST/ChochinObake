using System.Collections;
using UnityEngine;

public abstract class ClientManager<T> : ClientSingletonGameStateObserver<T> where T : Component
{

	#region Attributes

	private ManagersStates reference;

	#endregion

	#region Request

	public bool IsReady
	{
		get
		{
			return reference.state;
		}
	}

	#endregion

	#region Life cycle

	protected override void Awake()
	{
		base.Awake();

		reference = new ManagersStates();
	}

	protected virtual IEnumerator Start()
	{
		reference.state = false;
		yield return StartCoroutine(InitCoroutine());
		reference.state = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		reference.Destroy();
	}

	#endregion

	#region Tools

	protected abstract IEnumerator InitCoroutine();

	#endregion
}