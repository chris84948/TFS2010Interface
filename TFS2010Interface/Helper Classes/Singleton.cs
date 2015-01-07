using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chrisbjohnson.TFS2010Interface
{
    public class Singleton
    {
        protected static Singleton _obj;
        public event EventHandler OptionsUpdatedEvent;

        private string _tfsServer;
        private string _tfsWorkspace;
        private string _tfsPath;

        private Singleton()
        {

        }

        public static Singleton GetObject()
        {
            if (_obj == null)
            {
                _obj = new Singleton();
            }
            return _obj;
        }

        public string TFSServer
        {
            get { return _tfsServer; }
            set
            {
                if (value != _tfsServer)
                {
                    _tfsServer = value;
                    EventHandler handler = OptionsUpdatedEvent;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }

        }

        public string TFSWorkspace
        {
            get { return _tfsWorkspace; }
            set
            {
                if (value != _tfsWorkspace)
                {
                    _tfsWorkspace = value;
                    EventHandler handler = OptionsUpdatedEvent;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }

        }

        public string TFSPath
        {
            get { return _tfsPath; }
            set
            {
                if (value != _tfsPath)
                {
                    _tfsPath = value;
                    EventHandler handler = OptionsUpdatedEvent;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }

        }
    }
}
