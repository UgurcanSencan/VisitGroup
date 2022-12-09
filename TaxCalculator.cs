using System;
using System.Collections.Generic;
using System.Linq;

//The focus should be on clean, simple and easy to read code 
//Everything but the public interface may be changed
namespace TaxCalculatorInterviewTests
{
    /// <summary>
    /// This is the public inteface used by our client and may not be changed
    /// </summary>
    public interface ITaxCalculator
    {
        double GetStandardTaxRate(Commodity commodity);
        void SetCustomTaxRate(Commodity commodity, double rate);
        double GetTaxRateForDateTime(Commodity commodity, DateTime date);
        double GetCurrentTaxRate(Commodity commodity);
    }

    /// <summary>
    /// Implements a tax calculator for our client.
    /// The calculator has a set of standard tax rates that are hard-coded in the class.
    /// It also allows our client to remotely set new, custom tax rates.
    /// Finally, it allows the fetching of tax rate information for a specific commodity and point in time.
    /// TODO: We know there are a few bugs in the code below, since the calculations look messed up every now and then.
    ///       There are also a number of things that have to be implemented.
    /// </summary>
    public class TaxCalculator : ITaxCalculator
    {
        public TaxCalculator()
        {
            // To record multiple custom rates for each Commodity element at different DateTime
            foreach (Commodity com in (Commodity[])Enum.GetValues(typeof(Commodity)))
            {
                _customRates[com] = new List<Tuple<DateTime, double>>();
            }
        }

        /// <summary>
        /// Get the standard tax rate for a specific commodity.
        /// </summary>
        public double GetStandardTaxRate(Commodity commodity)
        {
            if (commodity == Commodity.Default)
                return 0.25;
            if (commodity == Commodity.Alcohol)
                return 0.25;
            if (commodity == Commodity.Food)
                return 0.12;
            if (commodity == Commodity.FoodServices)
                return 0.12;
            if (commodity == Commodity.Literature)
                return 0.6;
            if (commodity == Commodity.Transport)
                return 0.6;
            if (commodity == Commodity.CulturalServices)
                return 0.6;

            return 0.25;
        }


        /// <summary>
        /// This method allows the client to remotely set new custom tax rates.
        /// When they do, we save the commodity/rate information as well as the UTC timestamp of when it was done.
        /// NOTE: Each instance of this object supports a different set of custom rates, since we run one thread per customer.
        /// </summary>
        public void SetCustomTaxRate(Commodity commodity, double rate)
        {
            //TODO: support saving multiple custom rates for different combinations of Commodity/DateTime
            //TODO: make sure we never save duplicates, in case of e.g. clock resets, DST etc - overwrite old values if this happens
            DateTime now = DateTime.UtcNow;
            long nowUnix = new DateTimeOffset(now).ToUnixTimeSeconds();
            int index = _customRates[commodity].FindIndex(x => nowUnix == new DateTimeOffset(x.Item1).ToUnixTimeSeconds());

            if (index == -1)
                // Create new Tuple
                _customRates[commodity].Add(Tuple.Create(now, rate));
            else
                // Overwrite
                _customRates[commodity][index] = Tuple.Create(now, rate);

        }
        static Dictionary<Commodity, List<Tuple<DateTime, double>>> _customRates = new Dictionary<Commodity, List<Tuple<DateTime, double>>>();


        /// <summary>
        /// Gets the tax rate that is active for a specific point in time (in UTC).
        /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
        /// If there is no custom tax rate for the specified date, use the standard tax rate.
        /// </summary>
        public double GetTaxRateForDateTime(Commodity commodity, DateTime date)
        {
            int index = -1;
            long dateUnix = new DateTimeOffset(date).ToUnixTimeSeconds();
            long temp;

            for (int i = 0; i < _customRates[commodity].Count; i++)
            {
                temp = new DateTimeOffset(_customRates[commodity][i].Item1).ToUnixTimeSeconds();

                // Finding the index of currently active rate by timestamp.
                if (dateUnix >= temp)
                    index = i;
                else
                    break;
            }
            
            if (index == -1)
                // Getting the standard tax rate if there is no rate for the specified date.
                return GetStandardTaxRate(commodity);
            else
                // Getting the active custom tax rate for the specified date.
                return _customRates[commodity][index].Item2;
        }

        /// <summary>
        /// Gets the tax rate that is active for the current point in time.
        /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
        /// If there is no custom tax currently active, use the standard tax rate.
        /// </summary>
        public double GetCurrentTaxRate(Commodity commodity)
        {
            if (_customRates[commodity].Count == 0)
                // Getting the standard tax rate if there is no rate.
                return GetStandardTaxRate(commodity);
            else
                // Getting the current rate if there is a rate (at least one).
                return _customRates[commodity][_customRates[commodity].Count - 1].Item2;
        }

    }

    public enum Commodity
    {
        //PLEASE NOTE: THESE ARE THE ACTUAL TAX RATES THAT SHOULD APPLY, WE JUST GOT THEM FROM THE CLIENT!
        Default,            //25%
        Alcohol,            //25%
        Food,               //12%
        FoodServices,       //12%
        Literature,         //6%
        Transport,          //6%
        CulturalServices    //6%
    }
}
