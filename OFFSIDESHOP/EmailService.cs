using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace OFFSIDESHOP
{
    public static class EmailService
    {
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionDataBase"].ConnectionString;

        private struct SmtpConfig
        {
            public string SenderName;
            public string SenderEmail;
            public string AppPassword;
            public string Host;
            public int Port;
        }

        private static SmtpConfig GetSmtpSettings()
        {
            SmtpConfig config = new SmtpConfig();
            string query = "SELECT SenderName, SenderEmail, AppPassword, SmtpHost, SmtpPort FROM smtp_settings WHERE ID = 1";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            config.SenderName = reader["SenderName"].ToString();
                            config.SenderEmail = reader["SenderEmail"].ToString();
                            config.AppPassword = reader["AppPassword"].ToString().Replace(" ", "");
                            config.Host = reader["SmtpHost"].ToString();
                            config.Port = Convert.ToInt32(reader["SmtpPort"]);
                        }
                    }
                }
            }
            return config;
        }

        private static List<string> GetAdminAndOwnerEmails()
        {
            List<string> emails = new List<string>();
            string query = "SELECT Mail FROM users WHERE Id_Role IN (1, 2)";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emails.Add(reader["Mail"].ToString());
                        }
                    }
                }
            }
            return emails;
        }

        // ==========================================
        // NUEVO: ENVIAR TOKEN DE REGISTRO (SIGN UP)
        // ==========================================
        public static void SendRegistrationToken(string userEmail, string clientName, string randomCode)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                mail.To.Add(userEmail);
                mail.Subject = "Verify your Email Address";

                mail.Body = $@"
            <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 12px; background-color: #ffffff;'>
                <div style='text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f4f4f4;'>
                    <h1 style='color: #000000; margin: 0; font-size: 28px; letter-spacing: 1px;'>{smtp.SenderName.Replace("Shop", "")}<span style='color: #FFC800;'>Shop</span></h1>
                </div>
                <div style='padding: 30px 20px;'>
                    <h2 style='color: #333; margin-top: 0;'>Email Verification</h2>
                    <p style='color: #555; line-height: 1.6; font-size: 16px;'>Hello <strong>{clientName}</strong>,</p>
                    <p style='color: #555; line-height: 1.6; font-size: 16px;'>Thank you for creating an account with us. Please use the following verification code to activate your account:</p>
                    
                    <div style='text-align: center; margin: 40px 0;'>
                        <div style='display: inline-block; background-color: #f8f9fa; border: 2px dashed #FFC800; padding: 20px 40px; border-radius: 8px;'>
                            <span style='font-size: 32px; font-weight: bold; color: #000000; letter-spacing: 8px;'>{randomCode}</span>
                        </div>
                        <p style='color: #888; font-size: 14px; margin-top: 15px;'>Do not share this code with anyone.</p>
                    </div>
                </div>
                <div style='text-align: center; padding: 20px; border-top: 1px solid #f4f4f4; color: #999; font-size: 13px;'>
                    <p style='margin: 5px 0;'>&copy; {DateTime.Now.Year} {smtp.SenderName}. All rights reserved.</p>
                </div>
            </div>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = smtp.Host;
                    smtpClient.Port = smtp.Port;
                    smtpClient.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Registration Email Failed: " + ex.Message);
                throw;
            }
        }

        public static void SendRefundNotification(string customerEmail, string customerName, string idOrder, decimal orderTotal)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                // 1. Send Email to the Customer
                MailMessage customerMail = new MailMessage();
                customerMail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                customerMail.To.Add(customerEmail);
                customerMail.Subject = $"Refund Requested - Order #{idOrder}";
                customerMail.Body = $@"
                    <h3>Hello, {customerName}!</h3>
                    <p>We have received your refund request for order <strong>#{idOrder}</strong>.</p>
                    <p><strong>Refund Amount:</strong> ${orderTotal}</p>
                    <p>Our administration team is processing your cancelation. You will receive an email once it is approved.</p>
                    <br/>
                    <p>Best regards,<br/>The {smtp.SenderName} Team</p>";
                customerMail.IsBodyHtml = true;

                // 2. Send Email to Admin and Owners
                MailMessage adminMail = new MailMessage();
                adminMail.From = new MailAddress(smtp.SenderEmail, $"{smtp.SenderName} Notifications");

                List<string> adminEmails = GetAdminAndOwnerEmails();
                foreach (string email in adminEmails)
                {
                    adminMail.To.Add(email);
                }

                adminMail.Subject = $"ALERT: New Refund Requested - Order #{idOrder}";
                adminMail.Body = $@"
                    <h3>Attention Management Team,</h3>
                    <p>A user has just requested a refund and cancelation for an order.</p>
                    <ul>
                        <li><strong>Customer Name:</strong> {customerName}</li>
                        <li><strong>Customer Email:</strong> {customerEmail}</li>
                        <li><strong>Order ID:</strong> #{idOrder}</li>
                        <li><strong>Total Amount to Refund:</strong> ${orderTotal}</li>
                    </ul>
                    <p>Please log in to the administrator dashboard to approve or review this request.</p>";
                adminMail.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = smtp.Host;
                    smtpClient.Port = smtp.Port;
                    smtpClient.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(customerMail);
                    if (adminMail.To.Count > 0)
                    {
                        smtpClient.Send(adminMail);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email Sending Failed: " + ex.Message);
            }
        }

        public static void SendPasswordRecoveryToken(string userEmail, string clientName, string randomCode)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                mail.To.Add(userEmail);
                mail.Subject = "Password recovery";

                mail.Body = $@"
            <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 12px; background-color: #ffffff;'>
                <div style='text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f4f4f4;'>
                    <h1 style='color: #000000; margin: 0; font-size: 28px; letter-spacing: 1px;'>{smtp.SenderName.Replace("Shop", "")}<span style='color: #007bff;'>Shop</span></h1>
                </div>
                <div style='padding: 30px 20px;'>
                    <h2 style='color: #333; margin-top: 0;'>Password Reset Request</h2>
                    <p style='color: #555; line-height: 1.6; font-size: 16px;'>Hello <strong>{clientName}</strong>,</p>
                    <p style='color: #555; line-height: 1.6; font-size: 16px;'>We received a request to reset the password for your account. Please use the following verification code to complete the process:</p>
                    
                    <div style='text-align: center; margin: 40px 0;'>
                        <div style='display: inline-block; background-color: #f8f9fa; border: 2px dashed #007bff; padding: 20px 40px; border-radius: 8px;'>
                            <span style='font-size: 32px; font-weight: bold; color: #000000; letter-spacing: 8px;'>{randomCode}</span>
                        </div>
                        <p style='color: #888; font-size: 14px; margin-top: 15px;'>This code will expire in 2 minutes for your security.</p>
                    </div>

                    <p style='color: #555; line-height: 1.6; font-size: 16px;'>If you did not request this change, you can safely ignore this email. Your password will remain unchanged.</p>
                </div>
                <div style='text-align: center; padding: 20px; border-top: 1px solid #f4f4f4; color: #999; font-size: 13px;'>
                    <p style='margin: 5px 0;'>&copy; {DateTime.Now.Year} {smtp.SenderName}. All rights reserved.</p>
                    <p style='margin: 5px 0;'>This is an automated message, please do not reply.</p>
                </div>
            </div>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = smtp.Host;
                    smtpClient.Port = smtp.Port;
                    smtpClient.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(mail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Recovery Email Failed: " + ex.Message);
                throw;
            }
        }

        public static void SendOrderConfirmation(int orderId, decimal total, DataTable dtCart, string metodoPago, string shippingText, string shippingAddressHtml, string customerEmail, string customerName)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                string productosHtml = "";
                foreach (DataRow row in dtCart.Rows)
                {
                    productosHtml += $@"
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #f4f4f4;'>
                        <img src='images/camisetas/{row["ImageURL"]}' width='50' style='border-radius:4px; vertical-align: middle; margin-right: 10px;' />
                        {row["Quantity"]}x {row["Name"]} - Size: {row["Size"]}
                    </td>
                    <td style='padding: 10px; border-bottom: 1px solid #f4f4f4; text-align:right;'>
                        ${Convert.ToDecimal(row["Subtotal"]):F2}
                    </td>
                </tr>";
                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                mail.To.Add(customerEmail);
                mail.Subject = $"Order Confirmation #{orderId} - {smtp.SenderName}";

                mail.Body = $@"
            <div style='font-family: Segoe UI, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 12px;'>
                <div style='text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f4f4f4;'>
                    <h1 style='color: #000; margin: 0;'>{smtp.SenderName.Replace("Shop", "")}<span style='color: #FFC800;'>Shop</span></h1>
                </div>
                <div style='padding: 30px 20px;'>
                    <h2 style='color: #333;'>Order Confirmation #{orderId}</h2>
                    <p style='color: #555; font-size: 16px;'>Hello <strong>{customerName}</strong>, thank you for your order!</p>
                    
                    <h3 style='color: #333; margin-top: 25px; margin-bottom: 10px;'>Shipping Address</h3>
                    {shippingAddressHtml}
                    
                    <h3 style='color: #333; margin-top: 30px;'>Order Summary</h3>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr style='background: #f8f8f8;'>
                            <th style='padding: 10px; text-align:left;'>Product</th>
                            <th style='padding: 10px; text-align:right;'>Total</th>
                        </tr>
                        {productosHtml}
                        <tr>
                            <td style='padding: 10px;'><strong>Shipping Cost</strong></td>
                            <td style='padding: 10px; text-align:right;'><strong>{shippingText}</strong></td>
                        </tr>
                        <tr style='background: #f8f8f8;'>
                            <td style='padding: 10px;'><strong>TOTAL</strong></td>
                            <td style='padding: 10px; text-align:right; color: #FFC800;'><strong>${total:F2}</strong></td>
                        </tr>
                    </table>
                    
                    <h3 style='color: #333; margin-top: 30px;'>Payment Method</h3>
                    <p style='color: #555; font-size: 15px; background: #fdfaf0; padding: 10px; border-left: 4px solid #FFC800; border-radius: 4px;'>
                        <strong>{metodoPago}</strong>
                    </p>
                </div>
            </div>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = smtp.Host;
                    smtpClient.Port = smtp.Port;
                    smtpClient.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(mail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Order Email Failed: " + ex.Message);
            }
        }

        public static void SendRefundApprovedNotification(string userEmail, string userName, string orderNumber, decimal refundAmount, string adminComment)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                mail.To.Add(userEmail);
                mail.Subject = $"Refund Approved 🎉 - Order #{orderNumber}";

                mail.Body = $@"
            <div style='font-family: ""Segoe UI"", sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 12px;'>
                <div style='text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f4f4f4;'>
                    <h1 style='color: #000; margin: 0;'>{smtp.SenderName.Replace("Shop", "")}<span style='color: #FFC800;'>Shop</span></h1>
                </div>
                <div style='padding: 30px 20px;'>
                    <h2 style='color: #27ae60; margin-top: 0;'>Good news, {HttpUtility.HtmlEncode(userName)}!</h2>
                    <p style='color: #555; font-size: 15px; line-height: 1.6;'>Your refund request for order <strong>#{orderNumber}</strong> has been **Approved** by our administration team.</p>
                    
                    <div style='background: #f9f9f9; border-left: 4px solid #27ae60; padding: 12px; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 0 0 5px 0; color: #333;'><strong>Refunded Amount:</strong> ${refundAmount:F2} USD</p>
                        <p style='margin: 0; color: #555;'><strong>Resolution Notes:</strong> <em>{HttpUtility.HtmlEncode(adminComment)}</em></p>
                    </div>

                    <p style='color: #777; font-size: 14px; line-height: 1.5;'>If your payment was processed via PayPal, the funds should reflect in your balance shortly depending on your financial institution's processing pipelines.</p>
                </div>
                <div style='text-align: center; padding-top: 15px; border-top: 1px solid #f4f4f4; color: #999; font-size: 12px;'>
                    <p>Thank you for choosing {smtp.SenderName}.</p>
                </div>
            </div>";
                mail.IsBodyHtml = true;

                using (SmtpClient client = new SmtpClient(smtp.Host, smtp.Port))
                {
                    client.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    client.EnableSsl = true;
                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Approval Email Failed: " + ex.Message);
            }
        }

        public static void SendRefundDeniedNotification(string userEmail, string userName, string orderNumber, string adminComment)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                mail.To.Add(userEmail);
                mail.Subject = $"Update regarding your Refund Request - Order #{orderNumber}";

                mail.Body = $@"
            <div style='font-family: ""Segoe UI"", sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 12px;'>
                <div style='text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f4f4f4;'>
                    <h1 style='color: #000; margin: 0;'>{smtp.SenderName.Replace("Shop", "")}<span style='color: #FFC800;'>Shop</span></h1>
                </div>
                <div style='padding: 30px 20px;'>
                    <h2 style='color: #c0392b; margin-top: 0;'>Hello {HttpUtility.HtmlEncode(userName)},</h2>
                    <p style='color: #555; font-size: 15px; line-height: 1.6;'>We are writing to inform you that your refund request for order <strong>#{orderNumber}</strong> has been **Declined** after administrative evaluation.</p>
                    
                    <div style='background: #fff5f5; border-left: 4px solid #c0392b; padding: 12px; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 0; color: #c0392b; font-weight: bold;'>Reason for Denial:</p>
                        <p style='margin: 5px 0 0 0; color: #555;'>{HttpUtility.HtmlEncode(adminComment)}</p>
                    </div>

                    <p style='color: #555; font-size: 14px; line-height: 1.5;'>Your order status remains registered as <strong>Paid</strong>. If you believe this evaluation constitutes an operational oversight, please contact our helpline replying directly to this email.</p>
                </div>
                <div style='text-align: center; padding-top: 15px; border-top: 1px solid #f4f4f4; color: #999; font-size: 12px;'>
                    <p>&copy; {DateTime.Now.Year} {smtp.SenderName} Management Panel.</p>
                </div>
            </div>";
                mail.IsBodyHtml = true;

                using (SmtpClient client = new SmtpClient(smtp.Host, smtp.Port))
                {
                    client.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    client.EnableSsl = true;
                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Denial Email Failed: " + ex.Message);
            }
        }
        // ==========================================
        // NUEVO: RESPUESTA DE TICKETS (SUPPORT & SELLER)
        // ==========================================
        public static void SendTicketApprovedNotification(string userEmail, string ticketId, string subject, string adminResponse, bool isConsignment)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                mail.To.Add(userEmail);
                mail.Subject = $"Ticket #{ticketId} Resolved: {subject}";

                string userName = userEmail.Split('@')[0];

                // Si es consignación, añadimos un mensaje de felicitación especial
                string consignmentHtml = isConsignment
                    ? @"<div style='background: #e6f4ea; border-left: 4px solid #27ae60; padding: 15px; margin-bottom: 20px; border-radius: 4px;'>
                            <h3 style='color: #27ae60; margin: 0 0 10px 0;'><i class='fas fa-check-circle'></i> Consignment Approved!</h3>
                            <p style='margin: 0; color: #333; font-size: 15px;'>Congratulations! Your collector's jersey has been reviewed, approved, and added to the official OffsideShop catalog. We will contact you regarding payment as soon as the item is sold.</p>
                        </div>"
                    : "";

                // Formateamos los saltos de línea del TextBox para que se vean bien en HTML
                string formattedResponse = HttpUtility.HtmlEncode(adminResponse).Replace("\n", "<br/>");

                mail.Body = $@"
            <div style='font-family: ""Segoe UI"", sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 12px;'>
                <div style='text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f4f4f4;'>
                    <h1 style='color: #000; margin: 0;'>{smtp.SenderName.Replace("Shop", "")}<span style='color: #FFC800;'>Shop</span></h1>
                </div>
                <div style='padding: 30px 20px;'>
                    {consignmentHtml}
                    <h2 style='color: #333; margin-top: 0;'>Update on Ticket #{ticketId}</h2>
                    <p style='color: #555; font-size: 15px; line-height: 1.6;'>Hello <strong>{userName}</strong>,</p>
                    <p style='color: #555; font-size: 15px; line-height: 1.6;'>Our support team has reviewed your request regarding <em>""{subject}""</em>. Below is the official response:</p>
                    
                    <div style='background: #f8f9fa; border: 1px solid #e0e0e0; padding: 20px; margin: 25px 0; border-radius: 8px; color: #444; font-size: 15px; line-height: 1.6;'>
                        {formattedResponse}
                    </div>

                    <p style='color: #777; font-size: 14px; line-height: 1.5;'>If you have any further questions, feel free to reply directly to this email.</p>
                </div>
                <div style='text-align: center; padding-top: 15px; border-top: 1px solid #f4f4f4; color: #999; font-size: 12px;'>
                    <p>Thank you for trusting {smtp.SenderName}.</p>
                </div>
            </div>";
                mail.IsBodyHtml = true;

                using (SmtpClient client = new SmtpClient(smtp.Host, smtp.Port))
                {
                    client.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    client.EnableSsl = true;
                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ticket Approval Email Failed: " + ex.Message);
            }
        }

        public static void SendTicketDeniedNotification(string userEmail, string ticketId, string subject, string adminResponse)
        {
            try
            {
                SmtpConfig smtp = GetSmtpSettings();
                if (string.IsNullOrEmpty(smtp.SenderEmail)) return;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtp.SenderEmail, smtp.SenderName);
                mail.To.Add(userEmail);
                mail.Subject = $"Update on Ticket #{ticketId}: {subject}";

                string userName = userEmail.Split('@')[0];
                string formattedResponse = HttpUtility.HtmlEncode(adminResponse).Replace("\n", "<br/>");

                mail.Body = $@"
            <div style='font-family: ""Segoe UI"", sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 12px;'>
                <div style='text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f4f4f4;'>
                    <h1 style='color: #000; margin: 0;'>{smtp.SenderName.Replace("Shop", "")}<span style='color: #FFC800;'>Shop</span></h1>
                </div>
                <div style='padding: 30px 20px;'>
                    <h2 style='color: #c0392b; margin-top: 0;'>Ticket #{ticketId} Update</h2>
                    <p style='color: #555; font-size: 15px; line-height: 1.6;'>Hello <strong>{userName}</strong>,</p>
                    <p style='color: #555; font-size: 15px; line-height: 1.6;'>Our administration team has finalized the review of your request <em>""{subject}""</em>. Unfortunately, your request has been declined at this time.</p>
                    
                    <div style='background: #fff5f5; border-left: 4px solid #c0392b; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 0; color: #c0392b; font-weight: bold; margin-bottom: 10px;'>Reason / Administrator Notes:</p>
                        <p style='margin: 0; color: #555; line-height: 1.6;'>{formattedResponse}</p>
                    </div>

                    <p style='color: #555; font-size: 14px; line-height: 1.5;'>We appreciate your understanding. If you believe there has been a mistake or need further clarification, please reply to this email.</p>
                </div>
                <div style='text-align: center; padding-top: 15px; border-top: 1px solid #f4f4f4; color: #999; font-size: 12px;'>
                    <p>&copy; {DateTime.Now.Year} {smtp.SenderName} Support.</p>
                </div>
            </div>";
                mail.IsBodyHtml = true;

                using (SmtpClient client = new SmtpClient(smtp.Host, smtp.Port))
                {
                    client.Credentials = new NetworkCredential(smtp.SenderEmail, smtp.AppPassword);
                    client.EnableSsl = true;
                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ticket Denial Email Failed: " + ex.Message);
            }
        }
    }
}
