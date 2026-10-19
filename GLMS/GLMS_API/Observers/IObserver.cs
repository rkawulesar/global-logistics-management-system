using GLMS_API.Models;

namespace GLMS_API.Observers
{
    // Defines observer contract for when a service request updates
    public interface IObserver
    {
        void Update(ServiceRequest serviceRequest);
    }
}