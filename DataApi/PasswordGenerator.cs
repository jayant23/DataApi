using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess
{
    class PasswordGenerator
    {
        private char[] nonAlphaNumericCharArray = "`~!@#$%^&*()-_=+[]{}\\|;:'\",<.>/?".ToCharArray();
        private int passwordLength = 8; //Default is 8

        public PasswordGenerator()
        {
        }

        public PasswordGenerator(int passwordLength)
        {
            this.passwordLength = passwordLength;
        }

        /// <summary>
        /// Generates MD5 strong password including non alpha numeric character.
        /// </summary>
        /// <returns>The generated password.</returns>
        public string Generate()
        {
            //Get the MD5 random hash
            string md5Hash = MD5Hash();

            //create "passwordLength" characters long password from MD5 hash
            string trancatedPassword = md5Hash.Remove(this.passwordLength);

            //Get random non alphanumeric character
            int randomCharPosition = RandomNumber(nonAlphaNumericCharArray.GetLowerBound(0), nonAlphaNumericCharArray.GetUpperBound(0));
            char randomChar = nonAlphaNumericCharArray[randomCharPosition];
            int randomCharPositionInPassword = RandomNumber(0, trancatedPassword.Length);
            return trancatedPassword.Insert(randomCharPositionInPassword, randomChar.ToString());
        }

        /// <summary>
        /// This function generates random numbers in given range using
        /// cryptographically strong sequence of random values.
        /// </summary>
        /// <param name="minValue">The maximum return value</param>
        /// <param name="maxValue">The minimum return value</param>
        /// <returns>Random number between nim and max values</returns>
        public int RandomNumber(int minValue, int maxValue)
        {
            // Create a byte array to hold the random value.
            byte[] randomNumber = new byte[1];

            // Create a new instance of the RNGCryptoServiceProvider.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            // Fill the array with a random value.
            rng.GetBytes(randomNumber);

            // Convert the byte to an integer value to make the modulus operation easier.
            int rand = Convert.ToInt32(randomNumber[0]);

            return rand % (maxValue - minValue + 1) + minValue;
        }

        /// <summary>
        /// Perform MD5 hash and gererate coresponding string.
        /// </summary>
        /// <returns>Random string cryptographically MD5 strong</returns>
        private string MD5Hash()
        {
            byte[] rndBytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();


            rng.GetBytes(rndBytes); //fill the input array w/ random numbers

            byte[] hash = md5.ComputeHash(rndBytes);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }
    }
}
