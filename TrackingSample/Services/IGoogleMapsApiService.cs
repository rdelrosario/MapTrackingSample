using System.Threading.Tasks;
using TrackingSample.Models;

namespace TrackingSample.Services
{
    public interface IGoogleMapsApiService
    {
        Task<GoogleDirection> GetDirections(string originLatitude, string originLongitude, string destinationLatitude, string destinationLongitude);
    }
}
