﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LonghornBank.Models;

namespace LonghornBank.Controllers
{
    public class PayeesController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Payees
        public ActionResult Index()
        {
            return View(db.Payees.ToList());
        }

        // GET: Payees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // GET: Payees/Create
        public ActionResult Create()
        {
            var CustomerQuery = from c in db.Users
                                where c.UserName == User.Identity.Name
                                select c;


            // Get the Customer 
            AppUser customer = CustomerQuery.FirstOrDefault();

            if (customer.ActiveStatus == false)
            {
                return View("Frozen");
            }

            return View();
        }

        // POST: Payees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PayeeID,Name,StreetAddress,City,State,Zip,PhoneNumber,PayeeType")] Payee payee)
        { 


            if (ModelState.IsValid)
            {
                db.Payees.Add(payee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(payee);
        }

        // GET: Payees/Edit/5
        public ActionResult Edit(int? id)
        {
            var CustomerQuery = from c in db.Users
                                where c.UserName == User.Identity.Name
                                select c;


            // Get the Customer 
            AppUser customer = CustomerQuery.FirstOrDefault();

            if (customer.ActiveStatus == false)
            {
                return View("Frozen");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // POST: Payees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PayeeID,Name,StreetAddress,City,State,Zip,PhoneNumber,PayeeType")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(payee);
        }

        // GET: Payees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // POST: Payees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payee payee = db.Payees.Find(id);
            db.Payees.Remove(payee);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


       public ActionResult PayBillsPage()
        {
            // Query the Database for the logged in user 
            var CustomerQuery = from c in db.Users
                                where c.UserName == User.Identity.Name
                                select c;


            // Get the Customer 
            AppUser customer = CustomerQuery.FirstOrDefault();

            if (customer.ActiveStatus == false)
            {
                return View("Frozen");
            }

            if (customer == null)
            {
                return HttpNotFound();
            }

            // Populate a list of Checking Accounts
            var CheckingQuery = from ca in db.CheckingAccount
                                where ca.Customer.Id == customer.Id
                                select ca;

            // Create a list of accounts of execute
            List<Checking> AllCheckingAccounts = CheckingQuery.ToList();
            Checking SelectNone = new Checking() { CheckingID = 0, AccountNumber = "1000000000000", Balance = 0, Name = "None" };
            SelectNone.AccountDisplay = SelectNone.Name + " " + SelectNone.Balance;
            AllCheckingAccounts.Add(SelectNone);

            foreach (var account in AllCheckingAccounts)
            {
                account.AccountNumber = "XXXXXX" + account.AccountNumber.Substring(6);
                account.AccountDisplay = account.Name + " | " + account.Balance;
            }

            // Populate a list of Savings Accounts
            var SavingsQuery = from sa in db.SavingsAccount
                               where sa.Customer.Id == customer.Id
                               select sa;

            // Create a list of accounts of execute
            List<Saving> AllSavingsAccounts = SavingsQuery.ToList();

            Saving SelectNoneSavings = new Saving() { SavingID = 0, AccountNumber = "1000000000000", Balance = 0, Name = "None" };
            SelectNoneSavings.AccountDisplay = SelectNoneSavings.Name + " " + SelectNoneSavings.Balance;
            AllSavingsAccounts.Add(SelectNoneSavings);

            foreach (var account in AllSavingsAccounts)
            {
                account.AccountNumber = "XXXXXX" + account.AccountNumber.Substring(6);
                account.AccountDisplay =  account.Name + " | " + account.Balance;
            }


            PayeeViewModel PayeeCustomerInfo = new PayeeViewModel
            {
                UserCustomerProfile = customer,
                PayeeAccount = customer.PayeeAccounts,
                CheckingAccounts = new SelectList(AllCheckingAccounts, "CheckingID", "AccountDisplay"),
                SavingsAccounts = new SelectList(AllSavingsAccounts, "SavingID", "AccountDisplay")
            };

            return View(PayeeCustomerInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayBillsPage(PayeeViewModel Pay)
        {
            // get the customer 
            AppUser Customer = db.Users.Find(Pay.UserCustomerProfile.Id);

            if (Customer == null)
            {
                return RedirectToAction("Portal", "Home");
            }

                // create a new transaction list for the trade 
                List<BankingTransaction> PayeeTrans = new List<BankingTransaction>();

                Checking CustomerChecking = db.CheckingAccount.Find(Pay.CheckingID);
                
                Saving CustomerSaving = db.SavingsAccount.Find(Pay.SavingID);

                if(CustomerChecking == null && CustomerSaving == null)
                {
                    return RedirectToAction("Portal", "Home");
                }

             if(CustomerChecking != null)
             {
                if (CustomerChecking.Balance >= 0)
                {
                    List<Checking> CheckingList = new List<Checking>();

                    CheckingList.Add(CustomerChecking);

                    if (CustomerChecking.Balance - Pay.PayeeTransaction.Amount >= -50)
                    {
                        if (CustomerChecking.Balance - Pay.PayeeTransaction.Amount < 0 && CustomerChecking.Balance - Pay.PayeeTransaction.Amount >= -50)
                        {
                            BankingTransaction OverDrawn = new BankingTransaction
                            {
                                Amount = 30,
                                BankingTransactionType = BankingTranactionType.Fee,
                                TransactionDate = Pay.PayeeTransaction.TransactionDate,
                                CheckingAccount = CheckingList,
                                ApprovalStatus = ApprovedorNeedsApproval.Approved,
                                TransactionDispute = DisputeStatus.NotDisputed,
                                Description = "Over draw"
                            };

                            CustomerChecking.Overdrawn = true;

                            PayeeTrans.Add(OverDrawn);

                            db.BankingTransaction.Add(OverDrawn);
                            
                            db.SaveChanges();

                            // Send the email 
                            String Body = CustomerChecking.Name.ToString() + " : has been overdrawn and you have been charged a $30 fee. Your current account balance is $" + CustomerChecking.Balance.ToString();
                            LonghornBank.Utility.Email.PasswordEmail(Customer.Email, "Overdrawn Account", Body);

                        }

                        BankingTransaction CheckingWithdrawl = new BankingTransaction
                        {
                            Amount = Pay.PayeeTransaction.Amount,
                            BankingTransactionType = BankingTranactionType.BillPayment,
                            TransactionDate = Pay.PayeeTransaction.TransactionDate,
                            CheckingAccount = CheckingList,
                            Description = Pay.PayeeTransaction.Description,
                            ApprovalStatus = ApprovedorNeedsApproval.Approved,
                            TransactionDispute = DisputeStatus.NotDisputed
                        };

                        db.CheckingAccount.Find(CustomerChecking.CheckingID).Balance -= Pay.PayeeTransaction.Amount;

                        db.BankingTransaction.Add(CheckingWithdrawl);
                        db.SaveChanges();
                    }

                    else
                    {
                        return View("WithDrawalError");
                    }
                }

                else
                {
                    return View("BalanceError");
                }
            }

            if(CustomerSaving != null)
            {
                if (CustomerSaving.Balance >= 0)
                {
                    List<Saving> SavingList = new List<Saving>();

                    SavingList.Add(CustomerSaving);

                    if (CustomerSaving.Balance - Pay.PayeeTransaction.Amount >= -50)
                    {
                        if (CustomerSaving.Balance - Pay.PayeeTransaction.Amount < 0 && CustomerSaving.Balance - Pay.PayeeTransaction.Amount >= -50)
                        {
                            BankingTransaction OverDrawn = new BankingTransaction
                            {
                                Amount = 30,
                                BankingTransactionType = BankingTranactionType.Fee,
                                TransactionDate = Pay.PayeeTransaction.TransactionDate,
                                SavingsAccount = SavingList,
                                TransactionDispute = DisputeStatus.NotDisputed,
                                ApprovalStatus = ApprovedorNeedsApproval.Approved,
                                Description = "Over draw"
                            };

                            CustomerSaving.Overdrawn = true;

                            PayeeTrans.Add(OverDrawn);

                            db.BankingTransaction.Add(OverDrawn);
                            db.SaveChanges();

                            // Send the email 
                            String Body = CustomerSaving.Name.ToString() + " : has been overdrawn and you have been charged a $30 fee. Your current account balance is $" + CustomerSaving.Balance.ToString();
                            LonghornBank.Utility.Email.PasswordEmail(Customer.Email, "Overdrawn Account", Body);

                        }

                        BankingTransaction CheckingWithdrawl = new BankingTransaction
                        {
                            Amount = Pay.PayeeTransaction.Amount,
                            BankingTransactionType = BankingTranactionType.BillPayment,
                            TransactionDate = Pay.PayeeTransaction.TransactionDate,
                            SavingsAccount = SavingList,
                            Description = "Payment of bill",
                            ApprovalStatus = ApprovedorNeedsApproval.Approved,
                            TransactionDispute = DisputeStatus.NotDisputed
                            
                        };

                        db.SavingsAccount.Find(CustomerSaving.SavingID).Balance -= Pay.PayeeTransaction.Amount;

                        db.BankingTransaction.Add(CheckingWithdrawl);
                        db.SaveChanges();
                    }

                    else
                    {
                        return View("WithDrawalError");
                    }

                }

                else
                {
                    return View("BalanceError");
                }

            }

            return View("SuccessfulPayee");
        }

        public ActionResult EditOwnPayee(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Payee payee = db.Payees.Find(id);

            if (payee == null)
            {
                return HttpNotFound();
            }

            return View(payee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOwnPayee([Bind(Include = "PayeeID,Name,StreetAddress,City,State,Zip,PhoneNumber,PayeeType")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PayBillsPage");
            }
            return View(payee);
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
