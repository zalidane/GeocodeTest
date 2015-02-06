using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeocodeTest.Models;

namespace GeocodeTest.Controllers
{
    public class DistanceController : Controller
    {
        private AddressesContext context = new AddressesContext();
        private Double distanceFromSouthernBorder, distanceFromWesternBorder, distanceFromNorthernBorder, distanceFromEasternBorder;

        //
        // GET: /Distance/

        public ActionResult Distance(int id)
        {
            Address address = context.Addresses.Find(id);
            List<Double> distances = new List<double>();

            if (address.State.Equals("KS") || address.State.Equals("Kansas"))
            {
                if (address.Latitude.HasValue)
                {
                    distanceFromEasternBorder = CalculateDistance(address.Latitude.Value, address.Longitude.Value, address.Latitude.Value, Constants.EASTERNBORDER);
                    distanceFromNorthernBorder = CalculateDistance(address.Latitude.Value, address.Longitude.Value, Constants.NORTHERNBORDER, address.Longitude.Value);
                    distanceFromSouthernBorder = CalculateDistance(address.Latitude.Value, address.Longitude.Value, Constants.SOUTHERNBORDER, address.Longitude.Value);
                    distanceFromWesternBorder = CalculateDistance(address.Latitude.Value, address.Longitude.Value, address.Latitude.Value, Constants.WESTERNBORDER);

                    distanceFromEasternBorder = ConvertKilometersToMiles(distanceFromEasternBorder);
                    distanceFromNorthernBorder = ConvertKilometersToMiles(distanceFromNorthernBorder);
                    distanceFromSouthernBorder = ConvertKilometersToMiles(distanceFromSouthernBorder);
                    distanceFromWesternBorder = ConvertKilometersToMiles(distanceFromWesternBorder);

                    distances = new List<double> { distanceFromNorthernBorder, distanceFromSouthernBorder, distanceFromEasternBorder, distanceFromWesternBorder };
                }
            }

            return View(distances);
        }

        private double CalculateDistance(double cityLat, double cityLng, double borderLat, double borderLng)
        {
            /* Haversine Formula:
             *  a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
             *  c = 2 ⋅ atan2( √a, √(1−a) )
             *  d = R ⋅ c
             */
            double distance = 0.0;

            const int EARTH_RADIUS = 6371;  // km
            double φ1 = ConvertToRadians(cityLat);
            double φ2 = ConvertToRadians(borderLat);
            double Δφ = ConvertToRadians(borderLat - cityLat);
            double Δλ = ConvertToRadians(borderLng - cityLng);

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                       Math.Cos(φ1) * Math.Cos(φ2) *
                       Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            distance = EARTH_RADIUS * c;

            return distance;
        }

        private double ConvertToRadians(double coordinate)
        {
            return (Math.PI / 180) * coordinate;
        }

        private double ConvertKilometersToMiles(double distance)
        {
            return distance * 0.621371192;
        }
    }
}
