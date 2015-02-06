using GeocodeTest.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace GeocodeTest.Controllers
{
    public class AddressController : Controller
    {
        AddressesContext context = new AddressesContext();

        public ActionResult Index()
        {
            return View(context.Addresses);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Address a)
        {
            if (ModelState.IsValid)
            {
                string message = "";

                try
                {
                    string requestURI = string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false",
                                                    Uri.EscapeDataString(a.Address1 + " " + a.Address2 + " " + a.City + ", " + a.State + " " + a.Zip));

                    WebRequest request = WebRequest.Create(requestURI);
                    //WebRequest.DefaultWebProxy = proxy;
                    WebResponse response = request.GetResponse();
                    XDocument xdoc = XDocument.Load(response.GetResponseStream());

                    XElement result = xdoc.Element("GeocodeResponse").Element("result");

                    if (result!=null)
                    {
                        XElement locationElement = result.Element("geometry").Element("location");
                        XElement lat = locationElement.Element("lat");
                        XElement lng = locationElement.Element("lng");

                        IEnumerable<XElement> addressComponent = xdoc.Element("GeocodeResponse").Element("result").Elements("address_component");

                        a.Latitude = Double.Parse(lat.Value);
                        a.Longitude = Double.Parse(lng.Value);

                        if (String.IsNullOrWhiteSpace(a.Zip))
                        {
                            XElement zip = XElement.Parse(addressComponent.Last().FirstNode.ToString());
                            a.Zip = (zip.FirstNode as XText).Value;
                        }

                        context.Addresses.Add(a);
                        context.SaveChanges();
                    }
                    else
                    {
                        return View("Error");
                    }

                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException dbe)
                {
                    foreach (var eve in dbe.EntityValidationErrors)
                    {
                        string msg = String.Format("Entity of Type {0} in state {1} has the following validation errors:\n\r",
                                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);

                        foreach (var ve in eve.ValidationErrors)
                        {
                            msg += String.Format("- Property: {0}, Error: {1}\n\r", ve.PropertyName, ve.ErrorMessage);
                        }

                        message += msg;
                    }

                    return View("Error", message);
                }
                catch (Exception ex)
                {
                    return View("Error", ex.Message);
                }

                
            }
            return View();
        }

        public ActionResult Details(int id)
        {
            return View(context.Addresses.Find(id));
        }

        public ActionResult Edit(int id)
        {
            Address address = context.Addresses.Find(id);

            if (address == null)
            {
                return HttpNotFound();
            }

            return View(address);
        }

        [HttpPost]
        public ActionResult Edit(Address address)
        {
            if (ModelState.IsValid)
            {
                context.Entry(address).State = EntityState.Modified;
                context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(address);
        }

        public ActionResult Delete(int id)
        {
            Address address = context.Addresses.Find(id);

            if (address==null)
            {
                return HttpNotFound();
            }

            return View(address);
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            Address address = context.Addresses.Find(id);
            context.Addresses.Remove(address);
            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
