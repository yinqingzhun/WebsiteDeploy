using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AohPackageSubscriber
{
    public interface IDaemonService
    {
        void Start();

        void Stop();
    }
}
