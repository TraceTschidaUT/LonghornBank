﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LonghornBank.Models
{
    public class StockAccount
    {
        public Int32 StockAccountID { get; set; }

        [Display(Name = "Cash Balance")]
        [Required(ErrorMessage = "A Cash Balance is Required")]
        public Decimal CashBalance { get; set; }

        [Display(Name = "Account Name")]
        [Required(ErrorMessage = "An Account Name is Required")]
        public String Name { get; set; }

        [Display(Name = "Account Number")]
        public String AccountNumber { get; set; }

        private Decimal _decStockBalance;


        [Required(ErrorMessage = "A Stock Value is Required")]
        [Display(Name = "Stock Value")]
        public Decimal StockBalance
        {
            get
            {
                if (this.Trades.Count() != 0)
                {
                    foreach (var t in this.Trades)
                    {
                        _decStockBalance += (t.Quantity * t.PricePerShare);

                    }
                }

                else
                {
                    _decStockBalance = 0;
                }

                return _decStockBalance;
            }
        }

        [Required(ErrorMessage = "A Trading Fee is Required")]
        [Display(Name = "Trading Fee")]
        public Decimal TradingFee { get; set; }

        [Required(ErrorMessage = "Unrealized Gains are Required")]
        [Display(Name = "Unrealized Gains")]
        public Decimal Gains { get; set; }

        [Required(ErrorMessage = "A Bonus Portfolio Balance is Required")]
        [Display(Name = "Balanced Portfolio Bonus Balance")]
        public Decimal Bounses { get; set; }

        // A stock account can belong to 1 person 
        public virtual AppUser Customer { get; set; }

        // A stock account can have many trades
        public virtual List<Trade> Trades { get; set; }

        // Stock account can have many transactions 
        public virtual List<BankingTransaction> BankingTransaction { get; set; }
    }
}