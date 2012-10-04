using System;
using System.Collections.Generic;
using Nop.Plugin.Payments.SagePayServer.Domain;

namespace Nop.Plugin.Payments.SagePayServer.Services
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface ISagePayServerTransactionService
    {

        /// <summary>
        /// Gets a SagePay Server Transaction
        /// </summary>
        /// <param name="vendorTxId">The vendor transaction code</param>
        /// <returns>SagePayServerTransaction</returns>
        SagePayServerTransaction GetSagePayServerTransactionByVendorTxCode(string vendorTxCode);

        /// <summary>
        /// Inserts a SagePay Server Transaction
        /// </summary>
        /// <param name="sagePayServerTransaction">SagePay Server Transaction</param>
        void InsertSagePayServerTransaction(SagePayServerTransaction sagePayServerTransaction);

        /// <summary>
        /// Updates the SagePay Server Transaction
        /// </summary>
        /// <param name="sagePayServerTransaction">The SagePay Server Transaction</param>
        void UpdateSagePayServerTransaction(SagePayServerTransaction sagePayServerTransaction);



    }
}
