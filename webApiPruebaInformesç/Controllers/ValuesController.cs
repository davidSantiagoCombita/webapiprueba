using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using webApiPruebaInformes.Models;
using webApiPruebaInformes.Services;
using webApiPruebaInformesç.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace webApiPruebaInformes.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly ProcesosService _procesosServices;
        private readonly DataInfoInformesService _dataInfoInformesService;
        private ILogger<ValuesController> _log;

        public ValuesController(ProcesosService _procesosServices, DataInfoInformesService _dataInfoInformesService,
            ILogger<ValuesController> _log)
        {
            this._procesosServices = _procesosServices;
            this._dataInfoInformesService = _dataInfoInformesService;
            _log.LogInformation(": Comienza controlador general");
        }

        //obtiene todos los informes
        // GET: api/values
        [HttpGet]
        public async Task<List<Procesos>> Get() =>
            _procesosServices.GetProcesos();

        //Obtiene los informes ya creados por el usuario
        // GET api/values/
        [HttpGet]
        [Route("GetInformesCreados/{correo}")]
        public async Task<List<DataGenerarInforme>> GetDataInformes(string correo)=>
            _dataInfoInformesService.GetDataGenerateInformeClient(correo);

        [HttpGet]
        [Route("GetClientEmail")]
        public async Task<string> GetClientEmail() =>
            _dataInfoInformesService.GetCorreoClient();


        //Eliminar configuracion de informe creado previamente
        [HttpPost]
        [Route("DeleteInformConfig")]
        public async Task<string> DeleteInformConfig(InfoClientGetExcel info) =>
            _dataInfoInformesService.DeleteInformConfig(info.Id_client, info.Email);

        //Genera un informe de excel y lo envia por correo
        [HttpPost]
        [Route("GenerarInforme")]
        public async Task<ActionResult<string>> InformeGenerado(InfoClientGetExcel info)
        {
            return _dataInfoInformesService.GenerateInformeClient(info.Id_client, info.Email, _procesosServices.GetProcesos()); ; 
           
        }

        //Descarga el informe de excel generado previamente
        [HttpPost]
        [Route("DownloadInformGenerado")]
        public async Task<FileResult> DownloadInformGenerado(InfoClientGetExcel infoClient)
        {
            string paht = _dataInfoInformesService.fileDataInmformeExcel(infoClient.Id_client, infoClient.Email);
            FileContentResult file = new FileContentResult(
                System.IO.File.ReadAllBytes(paht),
                "application/xlsx")
            {
                FileDownloadName = "Informe.xlsx"
            };

            return file;

        }

        //Agrega un nuevo informe
        [HttpPost]
        [Route("AgregarInforme")]
        public async Task<string> AgregarInforme(DataGenerarInforme _newInforme) {

            return _dataInfoInformesService.InsertDataNewInforme(_newInforme);
        }

        //Actualiza un informe de la base de datos
        [HttpPut]
        [Route("ActualizarInforme/{id}")]
        public async Task<string> ActualizarInforme(int id, DataGenerarInforme _newInforme)
        {
            return _dataInfoInformesService.UpdateDataNewInforme(id,_newInforme);
        }

    }
}
