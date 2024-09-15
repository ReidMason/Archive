// https://forum.unity.com/threads/c-events-affecting-all-objects-with-attached-script-am-i-approaching-events-the-right-way.415707/

public delegate void EventDelegate(object sender, EventArgs args);
public delegate void EventDelegate<TArgs>(object sender, TArgs args) where TArgs : EventArgs;
public delegate void EventDelegate<TSender, TArgs>(TSender sender, TArgs args) where TArgs : EventArgs;
public abstract class EventArgs { }