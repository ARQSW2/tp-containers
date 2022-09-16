using MassTransit.Contracts.JobService;
using MassTransit;
using MassTransit.Futures.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UNAHUR.MessageFun.Business.Messaging;

namespace UNAHUR.MessageFun.Api.Controllers
{
    /// <summary>
    /// Send messages an get results
    /// </summary>
    [Route("api/message")]
    [ApiController]
    [AllowAnonymous]
    public class MessageController : ControllerBase
    {
        readonly IRequestClient<IDoJob> _client;
        readonly IRequestClient<IGreetingCmd> _greetingClient;
        readonly ILogger _logger;

        public MessageController(ILogger<MessageController> logger,IRequestClient<IDoJob> client, IRequestClient<IGreetingCmd> greetingClient)
        {
            _logger = logger;
            _client = client;
            _greetingClient = greetingClient;
        }

        /// <summary>
        /// Returns a simple message
        /// </summary>
        /// <returns>The string "Ok"</returns>
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok("Ok");
        }


        /// <summary>
        /// Crea un job batch
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("{data}")]
        public async Task<ActionResult<JobSubmissionAccepted>> CreateJob(string data)
        {


            var groupId = NewId.Next().ToString();

            _logger.LogInformation($"Sending job: {data}:{groupId}");


            Response<JobSubmissionAccepted> response = await _client.GetResponse<JobSubmissionAccepted>(new
            {
                data,
                groupId,
                Index = 0,
                Count = 1
            });



            return Ok(response.Message);
        }


        /// <summary>
        /// Ejemplo de CQRS
        /// </summary>
        /// <param name="name">Nombre</param>
        /// <returns>Un saludo procesado en el workes</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<GreetingCmdResponse>> GetGreetingFromWorker(string name)
        {


            Response<GreetingCmdResponse> response = await _greetingClient.GetResponse<GreetingCmdResponse>(new { name }, HttpContext.RequestAborted);

            return Ok(response.Message);
        }
    }
}
