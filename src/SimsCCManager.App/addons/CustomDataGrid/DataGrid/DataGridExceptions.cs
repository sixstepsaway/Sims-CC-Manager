using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataGridExceptions
{
    public class DataGridHeadersIncorrectException : Exception
    {
        /// <summary>
        /// Custom exception reporting for the data grid.
        /// </summary>
        public DataGridHeadersIncorrectException()
        {

        }

        public DataGridHeadersIncorrectException(string message)
            : base(message)
        {
        }

        public DataGridHeadersIncorrectException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}