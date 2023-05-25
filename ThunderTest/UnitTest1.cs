using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Thunder.Controllers;
using Thunder.Models;
using Thunder.Services;
using Xunit;
using Stripe;


namespace Thunder.Tests.Controllers
{
    public class WebhookControllerTests
    {
        private readonly WebhookController _webhookController;
        private readonly Mock<IUserService> _userServiceMock;

        public WebhookControllerTests()
        {
            // Create a mock for the IUserService
            _userServiceMock = new Mock<IUserService>();

            // Create an instance of the WebhookController and inject the mock IUserService
            _webhookController = new WebhookController(_userServiceMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsOkResult_ForPaymentIntentSucceededEvent()
        {
            // Arrange
            var json = ""; // Provide the JSON payload for the event

            // Act
            var result = await _webhookController.Index(json);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal("User PaymentIntent Succeeded.", okResult.Value);
        }

        [Fact]
        public async Task Index_ReturnsOkResult_ForPaymentIntentCanceledEvent()
        {
            // Arrange
            var json = ""; // Provide the JSON payload for the event

            // Act
            var result = await _webhookController.Index(json);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal("User PaymentIntent Canceled.", okResult.Value);
        }

        [Fact]
        public async Task Index_ReturnsOkResult_ForChargeSucceededEvent()
        {
            // Arrange
            var json = "{\r\n  \"id\": \"ch_3N0ZZeDHpayIZlcA1detFNzQ\",\r\n  \"object\": \"charge\",\r\n  \"amount\": 2000,\r\n  \"amount_captured\": 2000,\r\n  \"amount_refunded\": 0,\r\n  \"application\": null,\r\n  \"application_fee\": null,\r\n  \"application_fee_amount\": null,\r\n  \"balance_transaction\": \"txn_1N2OtzDHpayIZlcAeV9Et67u\",\r\n  \"billing_details\": {\r\n    \"address\": {\r\n      \"city\": null,\r\n      \"country\": null,\r\n      \"line1\": null,\r\n      \"line2\": null,\r\n      \"postal_code\": null,\r\n      \"state\": null\r\n    },\r\n    \"email\": null,\r\n    \"name\": null,\r\n    \"phone\": null\r\n  },\r\n  \"calculated_statement_descriptor\": \"THUNDRSHIP.COM\",\r\n  \"captured\": true,\r\n  \"created\": 1682381858,\r\n  \"currency\": \"usd\",\r\n  \"customer\": null,\r\n  \"description\": \"(created by Stripe CLI)\",\r\n  \"disputed\": false,\r\n  \"failure_balance_transaction\": null,\r\n  \"failure_code\": null,\r\n  \"failure_message\": null,\r\n  \"fraud_details\": {},\r\n  \"invoice\": null,\r\n  \"livemode\": false,\r\n  \"metadata\": {},\r\n  \"on_behalf_of\": null,\r\n  \"outcome\": {\r\n    \"network_status\": \"approved_by_network\",\r\n    \"reason\": null,\r\n    \"risk_level\": \"normal\",\r\n    \"risk_score\": 60,\r\n    \"seller_message\": \"Payment complete.\",\r\n    \"type\": \"authorized\"\r\n  },\r\n  \"paid\": true,\r\n  \"payment_intent\": \"pi_3N0ZZeDHpayIZlcA1rxm5HVH\",\r\n  \"payment_method\": \"pm_1N0ZZeDHpayIZlcA7zkomORu\",\r\n  \"payment_method_details\": {\r\n    \"card\": {\r\n      \"brand\": \"visa\",\r\n      \"checks\": {\r\n        \"address_line1_check\": null,\r\n        \"address_postal_code_check\": null,\r\n        \"cvc_check\": null\r\n      },\r\n      \"country\": \"US\",\r\n      \"exp_month\": 4,\r\n      \"exp_year\": 2024,\r\n      \"fingerprint\": \"rpGsSOg6ek097bxI\",\r\n      \"funding\": \"credit\",\r\n      \"installments\": null,\r\n      \"last4\": \"4242\",\r\n      \"mandate\": null,\r\n      \"network\": \"visa\",\r\n      \"network_token\": {\r\n        \"used\": false\r\n      },\r\n      \"three_d_secure\": null,\r\n      \"wallet\": null\r\n    },\r\n    \"type\": \"card\"\r\n  },\r\n  \"receipt_email\": null,\r\n  \"receipt_number\": null,\r\n  \"receipt_url\": \"https://pay.stripe.com/receipts/payment/CAcaFwoVYWNjdF8xTXhGQ25ESHBheUlabGNBKKStiqMGMgZAz4TSuQM6LBZTXpF8pFmy3cvgTjgTzYbgkN6uxJrm107_gC_QKuJSZssCH6pASry1LqCi\",\r\n  \"refunded\": false,\r\n  \"refunds\": {\r\n    \"object\": \"list\",\r\n    \"data\": [],\r\n    \"has_more\": false,\r\n    \"url\": \"/v1/charges/ch_3N0ZZeDHpayIZlcA1detFNzQ/refunds\"\r\n  },\r\n  \"review\": null,\r\n  \"shipping\": {\r\n    \"address\": {\r\n      \"city\": \"San Francisco\",\r\n      \"country\": \"US\",\r\n      \"line1\": \"510 Townsend St\",\r\n      \"line2\": null,\r\n      \"postal_code\": \"94103\",\r\n      \"state\": \"CA\"\r\n    },\r\n    \"carrier\": null,\r\n    \"name\": \"Jenny Rosen\",\r\n    \"phone\": null,\r\n    \"tracking_number\": null\r\n  },\r\n  \"source_transfer\": null,\r\n  \"statement_descriptor\": null,\r\n  \"statement_descriptor_suffix\": null,\r\n  \"status\": \"succeeded\",\r\n  \"transfer_data\": null,\r\n  \"transfer_group\": null\r\n}"; // Provide the JSON payload for the event
            var charge = new Charge
            {
                BillingDetails = new ChargeBillingDetails { Name = "John Doe" },
                AmountCaptured = 1000
            };

            _userServiceMock.Setup(x => x.UpdateUserBalanceByUserIdAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(new ServiceResult { Succeeded = true });

            // Act
            var result = await _webhookController.Index(json);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal("User balance updated successfully.", okResult.Value);

            _userServiceMock.Verify(x => x.UpdateUserBalanceByUserIdAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
        }

        [Fact]
        public async Task Index_ReturnsOkResult_ForChargeFailedEvent()
        {
            // Arrange
            var json = ""; // Provide the JSON payload for the event
            var charge = new Charge
            {
                BillingDetails = new BillingDetails { Name = "John Doe" },
                Amount = 1000
            };

            // Act
            var result = await _webhookController.Index(json);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal("Charge failed event received.", okResult.Value);
        }
    }
}
