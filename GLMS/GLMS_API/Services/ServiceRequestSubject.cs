using GLMS_API.Models;
using GLMS_API.Observers;
namespace GLMS_API.Services
{
    // Code Attribution for Observer design pattern implementation
    // Link: https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern

    // Acts as the Subject in the Observer pattern, managing observers and sending notifications on updates
    public class ServiceRequestSubject
    {
        private readonly List<IObserver> _observers = new();
        private readonly List<string> _notifications;

        public ServiceRequestSubject(List<string> notifications)
        {
            _notifications = notifications;
        }

        public IReadOnlyList<string> Notifications => _notifications;

        // Adds the observer
        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }

        // Removes the observer
        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        // Notify all observers when a change occurs    
        public void NotifyObservers(ServiceRequest serviceRequest)
        {
            foreach (var observer in _observers)
            {
                observer.Update(serviceRequest);
            }
        }
    }
}