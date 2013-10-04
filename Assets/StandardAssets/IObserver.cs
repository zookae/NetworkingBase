using System;

//Unity is only on .NET 2.0; IObserver is from 4.0
//http://msdn.microsoft.com/en-us/library/ff648108.aspx

public interface IObserver<T>
{
    //void Update( object subject );
    void OnNext( T value );

    //void Subscribe( IObservable<T> provider );
    
}
