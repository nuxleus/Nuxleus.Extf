using System;
using com.amazon.s3;

namespace Extf.Net
{
    public partial class GlobalClip
    {
        
        public AWSAuthConnection Connect
        {
            get
            {
                return this._Connect;
            }
            set
            {

                this._Connect = value;
            }
        }
        private bool _Connected = false;
        private bool Connected
        {
            get
            {
                return this._Connected;
            }
            set
            {

                this._Connected = value;
            }
        }
        
        public string BaseHost
        {
            get
            {
                return this._BaseHost;
            }
            set
            {
                this._BaseHost = value;
            }
        }

        

        public string StorageBase
        {
            get
            {
                return this._StorageBase;
            }
            set
            {
                this._StorageBase = value;
            }
        }

        

        public string FilePrefix
        {
            get
            {
                return this._FilePrefix;
            }
            set
            {
                this._FilePrefix = value;
            }
        }

        
        public string Guid
        {
            get
            {
                return this._Guid;
            }
            set
            {
                this._Guid = value;
            }
        }

        private string KeyPrefix
        {
            get
            {
                return this.FilePrefix + this.Guid;
            }
        }

        public AWSAuthConnection Clipboard()
        {
            if (Connected) return Connect;
            else
            {
                this.Init();
                this.Connected = true;
                return Connect;

            }
        }


    }
}
