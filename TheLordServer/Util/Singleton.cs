using System;

namespace TheLordServer.Util
{
    public class Singleton<T> where T : new()
    {
        private static T instance;
        private static readonly Object _sync_root = new object ( );

        public static T Instance {
            get
            {
                if ( instance == null )
                {
                    lock ( _sync_root )
                    {
                        if ( instance == null )
                        {
                            instance = new T ( );
                        }
                    }
                }
                return instance;
            }
        }
    }
}
