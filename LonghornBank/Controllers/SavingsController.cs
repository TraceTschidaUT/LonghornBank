﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LonghornBank.Models;
using LonghornBank.Utility;

namespace LonghornBank.Controllers
{
    public class SavingsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Savings
        public ActionResult Index()
        {
            // Query the Customer
            var CustomerQuery = from c in db.Users
                                    where c.UserName == User.Identity.Name
                                    select c;

            AppUser customer = CustomerQuery.FirstOrDefault();
            if (customer == null)
            {
                return HttpNotFound();

            }

            // Add the Customer to ViewBag to Access information 
            ViewBag.CustomerAccount = customer;

            // Select The Savings Accounts Associated with this customer 
            var SavingAccountQuery = from sa in db.SavingsAccount
                                       where sa.Customer.Id == customer.Id
                                       select sa;

            // Create list and execute the query 
            List<Saving> CustomerSaving = SavingAccountQuery.ToList();

            return View(CustomerSaving);
        }

        // GET: Savings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Saving saving = db.SavingsAccount.Find(id);
            if (saving == null)
            {
                return HttpNotFound();
            }

            // Get the List off all of the Banking Transaction For this Account 
            List<BankingTransaction> SavingTransactions = saving.BankingTransactions.ToList();

            // Pass the List to the ViewBag
            ViewBag.SavingsTransactions = SavingTransactions;
            ViewBag.Ranges = SearchTransactions.AmountRange();
            ViewBag.Dates = SearchTransactions.DateRanges();
            ViewBag.ResultsCount = SavingTransactions.Count;
            return View(saving);
        }

        public ActionResult Search(SearchViewModel TheSearch, Int32 SavingID)
        {
            Saving saving = db.SavingsAccount.Find(SavingID);
            List<BankingTransaction> Transactions = SearchTransactions.Search(db, TheSearch, 2, SavingID);

            // Add the list to the view bag
            ViewBag.SavingsTransactions = Transactions;
            ViewBag.Ranges = SearchTransactions.AmountRange();
            ViewBag.Dates = SearchTransactions.DateRanges();
            ViewBag.ResultsCount = Transactions.Count;
            return View("Details", saving);
        }

        // GET: Savings/Create
        public ActionResult Create()
        {
            // Query the Customer
            var CustomerQuery = from c in db.Users
                                where c.UserName == User.Identity.Name
                                select c;

            AppUser customer = CustomerQuery.FirstOrDefault();

            //Return frozen view if no go
            if (customer.ActiveStatus == false)
            {
                return View("Frozen");
            }

            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerID = customer.Id;
            return View();
        }

        // POST: Savings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SavingID, Balance, Name")] Saving saving)
        {
            // Query the Customer
            var CustomerQuery = from c in db.Users
                                where c.UserName == User.Identity.Name
                                select c;

            AppUser customer = CustomerQuery.FirstOrDefault();

            int new_account_number = Convert.ToInt32(db.SavingsAccount.Count()) + 1000000000;
            if (customer == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                saving.Customer = customer;

                // Auto incremenment the account number
                saving.AccountNumber = Utility.AccountNumber.AutoNumber(db);

                if (saving.Name == null)
                {
                    saving.Name = "Longhorn Savings";
                }

                db.SavingsAccount.Add(saving);
                db.SaveChanges();

                // check to see if the deposit amount is over $5000
                ApprovedorNeedsApproval FirstDeposit;
                if (saving.Balance > 5000m)
                {
                    FirstDeposit = ApprovedorNeedsApproval.NeedsApproval;
                    //Added by Carson 5/2
                    saving.PendingBalance = saving.Balance;
                    saving.Balance = 0;
                }
                else
                {
                    FirstDeposit = ApprovedorNeedsApproval.Approved;
                }
                var SavingQuery = from sa in db.SavingsAccount
                                    where sa.AccountNumber == saving.AccountNumber
                                    select sa;

                Saving CustomerSavingAccount = SavingQuery.FirstOrDefault();
                List<Saving> CustomerSavingList = new List<Saving>();
                CustomerSavingList.Add(CustomerSavingAccount);

                // Create a new transaction 
                BankingTransaction InitialDeposit = new BankingTransaction
                {
                    Amount = saving.Balance,
                    ApprovalStatus = FirstDeposit,
                    BankingTransactionType = BankingTranactionType.Deposit,
                    Description = "Initial Deposit to Cash Balance",
                    TransactionDate = DateTime.Today,
                    TransactionDispute = DisputeStatus.NotDisputed,
                    SavingsAccount = CustomerSavingList
                };

                // Add the transaction to the database
                db.BankingTransaction.Add(InitialDeposit);
                db.SaveChanges();
                return RedirectToAction("Portal", "Home");
            }

            return View(saving);
        }

        // GET: Savings/Edit/5
        public ActionResult Edit(int? id)
        {
            var CustomerQuery = from c in db.Users
                                where c.UserName == User.Identity.Name
                                select c;


            // Get the Customer 
            AppUser customer = CustomerQuery.FirstOrDefault();

            //Return frozen view if no go
            if (customer.ActiveStatus == false)
            {
                return View("Frozen");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Saving saving = db.SavingsAccount.Find(id);
            if (saving == null)
            {
                return HttpNotFound();
            }
            return View(saving);
        }

        // POST: Savings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SavingID, Name, AccountNumber")] Saving saving)
        {
            if(ModelState.IsValid)
            {
                // Find the CustomerID Associated with the Account
                var SavingCustomerQuery = from sa in db.SavingsAccount
                                            where sa.SavingID == saving.SavingID
                                            select sa.Customer.Id;


                // Execute the Find
                List<String> CustomerID = SavingCustomerQuery.ToList();

                String IntCustomerID = CustomerID[0];

                Saving CustomerSaving = db.SavingsAccount.Find(saving.SavingID);

                // updated the checking account 
                CustomerSaving.Name = saving.Name;

                db.Entry(CustomerSaving).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Portal", "Home", new { id = IntCustomerID });
            }
            return RedirectToAction("Index", "Savings", new { id = saving.SavingID });
        }

        // GET: Savings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Saving saving = db.SavingsAccount.Find(id);
            if (saving == null)
            {
                return HttpNotFound();
            }
            return View(saving);
        }

        // POST: Savings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Find the CustomerID Associated with the Account
            var SavingCustomerQuery = from sa in db.SavingsAccount
                                        where sa.SavingID == id
                                        select sa.Customer.Id;

            // Execute the Find
            List<String> CustomerID = SavingCustomerQuery.ToList();

            String IntCustomerID = CustomerID[0];

            Saving saving = db.SavingsAccount.Find(id);
            db.SavingsAccount.Remove(saving);
            db.SaveChanges();
            return RedirectToAction("Portal", "Home", new { id = IntCustomerID });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
