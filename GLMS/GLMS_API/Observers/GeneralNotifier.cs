using GLMS_API.Models;

namespace GLMS_API.Observers
{
    // Observer that logs service request updates
    public class GeneralNotifier : IObserver
    {
        private readonly List<string> _notifications;

        public GeneralNotifier(List<string> notifications)
        {
            _notifications = notifications;
        }

        // Adds a notification message when a service request is updated
        public void Update(ServiceRequest serviceRequest)
        {
            _notifications.Add(
              $"Service Request \"{serviceRequest.Description}\" for " +
              $"{serviceRequest.Contract?.Client?.FirstName} {serviceRequest.Contract?.Client?.Surname} " +
              $"was updated to status: {serviceRequest.Status}"
            );
        }
    }
}