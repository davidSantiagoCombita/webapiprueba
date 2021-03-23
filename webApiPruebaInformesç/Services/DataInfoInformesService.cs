using MongoDB.Driver;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using webApiPruebaInformes.Models;
using webApiPruebaInformesç.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Threading;

namespace webApiPruebaInformes.Services
{
    public class DataInfoInformesService
    {
        //email para enviar correos
        private const string emailSendEmail = "david.combita@monolegal.co";
        private const string contraseniaCorreoSend = "Clapi3105";


        private const string emailClientLogin = "paula@monolegal.co";


        //variable de conexion con la base de datos
        private readonly IMongoCollection<DataGenerarInforme> _generateInformes;

        //variables para intentar varias el envio de documentos
        private const int NumberOfRetries = 5;//numeno de intentos
        private const int DelayOnRetry = 1000;//tiempo entre intentos 1 seg

        public DataInfoInformesService(IContextInfoInformes settings) {
            var cliente = new MongoClient(settings.ConnectionString);
            var database = cliente.GetDatabase(settings.DatabaseName);
            _generateInformes = database.GetCollection<DataGenerarInforme>(settings.GenerateInformesCollectionName);
        }

        //devolver correo del cliente
        public string GetCorreoClient() {
            return emailClientLogin;
        }

        public List<DataGenerarInforme> GetDataGenerateInformeClient(string correo)
        {
            return  _generateInformes.Find(x => x.Email == correo).ToList();
        }

        //Agregar un nuevo informe a la base de datos
        public string InsertDataNewInforme(DataGenerarInforme data) {
            _generateInformes.InsertOne(data);
            return "Add complete";
        }

        //actualizar un informe que exta en la base de datos
        public string UpdateDataNewInforme(int id,DataGenerarInforme newData)
        {
            try {
                List<DataGenerarInforme> datalist = _generateInformes.Find(x => x.Email == newData.Email).ToList();
                DataGenerarInforme dataUpdate = datalist[id];
                newData.Id = dataUpdate.Id;
                var filter = Builders<DataGenerarInforme>.Filter.Eq("Id", dataUpdate.Id);
                _generateInformes.ReplaceOne(filter, newData);
                return "Update Fine";
            }
            catch (Exception e) {
                return e.Message.ToString();
            }
        }

        //Genera informe de excel y lo envia por correo
        public string GenerateInformeClient(int id_infoCreado, string correo, List<Procesos> procesos) {
            for (int x = 1; x <= NumberOfRetries; ++x)
            {
                try
                {
                    DataGenerarInforme dataInform = _generateInformes.Find(x => x.Email == correo).ToList()[id_infoCreado];

                    string patyhexcel = AppDomain.CurrentDomain.BaseDirectory + "Informe" + dataInform.Id + ".xlsx";

                    SLDocument document = new SLDocument();
                    DataTable _datatable = new DataTable();

                    EncabezadoCampos(_datatable, dataInform);
                    AgregarListaProcesos(OrdenarListaDeProcesos(procesos, dataInform), _datatable, procesos, dataInform);

                    document.ImportDataTable(1, 1, _datatable, true);

                    //Le da estilos al excel
                    createStyleExcel(document);
                    //guardar el excel
                    document.SaveAs(patyhexcel);
                    

                    //enviar y actualizar en la base de datos para poder descargar el excel
                    return enviarActualizarExcel(dataInform);
                }
                catch (Exception e) when (x <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                    
                }
            }

            return "No se pudo en ningun intento. Intentelo más tarde: ";
        }

        //enviar y actualizar en la basede datos para poder descargar el excel
        public string enviarActualizarExcel(DataGenerarInforme dataInform) {
            for (int y = 1; y <= NumberOfRetries; ++y)
            {
                try
                {
                    //cambia la informacion de la base de datos para porder descargar el archivo
                    UpdateDataInformGenerate(dataInform);
                    string filename = AppDomain.CurrentDomain.BaseDirectory + "Informe" + dataInform.Id + ".xlsx";
                    Attachment data_atach = new Attachment(filename, MediaTypeNames.Application.Octet);

                    if (EnviarCorreos(data_atach, dataInform))
                    {
                        return "Se enviaron y se creo el excel requerido.";
                    }
                    else
                    {
                        return "Error insperado en el envio del correo, se estara comunicando, igual puede obtener el informe descargandolo en la pagina.";
                    }

                }
                catch (Exception e)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }
            return "No se pudo en ningun intento. Intentelo más tarde";
        }

        //crear los estilos al excel que se esta generando
        public void createStyleExcel(SLDocument document) {
            SLStyle style = document.CreateStyle();
            style.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            style.Fill.SetPattern(PatternValues.Solid, SLThemeColorIndexValues.Accent1Color, SLThemeColorIndexValues.Accent1Color);
            style.SetVerticalAlignment(VerticalAlignmentValues.Center);

            for (int i = 1; i < 10; i++)
            {
                document.SetCellStyle(1, i, style);
            }
        }

        //Eliminar la configuracion de un informe creado con anterioridad y devolver la lista de configuracion
        //de informes creados
        public string DeleteInformConfig(int id_client, string email)
        {
            try {
                DataGenerarInforme dataInform = _generateInformes.Find(x => x.Email == email).ToList()[id_client];
                _generateInformes.DeleteOne(x => x.Id == dataInform.Id);
                //obtenermos y devolvemos los datos actualizados
                return "Se elimino correctamente";
            }
            catch (Exception e) {
                return e.Message.ToString();
            }
            
        }

        //Envia el path del archivo de excel que le pertenerce al informe a buscar
        public string fileDataInmformeExcel(int id, string correo) {
            List<DataGenerarInforme> datalist = _generateInformes.Find(x => x.Email == correo).ToList();
            DataGenerarInforme data = datalist[id];
            string filename = AppDomain.CurrentDomain.BaseDirectory+ "Informe" + data.Id + ".xlsx";
            return filename;
        }

        //Cambia la a true el valor del excel generado para que se pueda descargar
        private void UpdateDataInformGenerate(DataGenerarInforme data)
        {
            var filter = Builders<DataGenerarInforme>.Filter.Eq("Id", data.Id);
            var update = Builders<DataGenerarInforme>.Update.Set("InformeGenerado", true);
            var options = new FindOneAndUpdateOptions<DataGenerarInforme> { IsUpsert = true, ReturnDocument = ReturnDocument.After };

            _generateInformes.FindOneAndUpdate(
               filter, update, options);
            //Se hizo el update
        }

        //Crea un nuevo informe de procesos y lo agrega a la base de datos
        private void AgregarListaProcesos(IOrderedEnumerable<Procesos> ordenarProcesos, DataTable datatable, List<Procesos> procesos, DataGenerarInforme camposInforme)
        {
            if (ordenarProcesos != null)
            {
                if (camposInforme.CamposRequeridos.Length>0) {
                    foreach (Procesos proces_ordenados in ordenarProcesos)
                    {
                       datatable.Rows.Add(proces_ordenados.NumProceso);
                    }
                }
                
            }
            else {
                if (camposInforme.CamposRequeridos.Length > 0)
                {
                    //cuando no se sabe como se ordenan por cualquier error
                    foreach (Procesos proces_no_ordenados in procesos)
                    {
                        datatable.Rows.Add(proces_no_ordenados.NumProceso);
                    }
                }
            }
            
        }

        //vuelve un string las ultimas 5 actuaciones para ponerlas en el excel
        private string agreparProcesos(string[] ultimas5Actuaciones)
        {
            string actualizaciones = "";
            for (int i =0; i< ultimas5Actuaciones.Length;i++) {
                actualizaciones += ultimas5Actuaciones[i] + ", ";
            }
            return actualizaciones;
        }

        //Envia el informe generado por correo
        public bool EnviarCorreos(Attachment data,  DataGenerarInforme dataInform)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = emailSendEmail,
                Password = contraseniaCorreoSend

            };
            try {
                MailMessage mail = new MailMessage();
                smtpClient.EnableSsl = true;
                //correo de quien lo envia
                mail.From = new MailAddress(emailSendEmail);
                //corrreo a quien se lo envia
                mail.To.Add(new MailAddress(emailClientLogin));
                if (dataInform.Correos_Adicionales != "" && dataInform.Correos_Adicionales != null) {
                    mail.To.Add(new MailAddress(dataInform.Correos_Adicionales));
                    //buscar la forma d enviar correos adicionales
                }
                mail.Subject = "Monolegal - Nuevo informe: "+ dataInform.Titulo;
                mail.IsBodyHtml = true;
                //genera cuerpo del correo
                mail.Body = createBody(dataInform);
                if (data != null) {
                    mail.Attachments.Add(data);
                }               
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Send(mail);

                return true;
            }
            catch (Exception e) {
                return false;
            }
        }

        //crea el cuerpo del email a enviar
        public string createBody(DataGenerarInforme dataInform) { 
            return "<h4>Nuevo Informe generado correctamente</h4> " +
                       "<p>Adjunto encontrarás el informe generado correctamente en nuestro sistema con los siguientes criterios:</p>" +
                       "<ol> <li>Filtrado por:" + dataInform.FiltrarPor + "</li>" +
                       "<li>Ordenado por: " + dataInform.OrdenadoPor + "</li> </ol> " +
                       "<p> Te recordamos que puedes configurar el informe para que se adapte a tus necesidades," +
                       " por ejemplo modificar el orden de los procesos o de las columnas dentro del informe</p>" +
                       "<p>Para configurar el informe accede a tu Panel de Informes en este enlace: https://www.monolegal.co/MisInformes </p>";
        }

        //Ordena los procesos segun se le indique 
        private IOrderedEnumerable<Procesos> OrdenarListaDeProcesos(List<Procesos> procesos, DataGenerarInforme encabezadosinforme)
        {
            if (encabezadosinforme.OrdenadoPor == "termino")
            {
                return procesos.OrderBy(x => x.FechaRegistro);
            }
            else if (encabezadosinforme.OrdenadoPor == "primeroCambios")
            {
                return procesos.OrderBy(x => x.UltimaFechaActuacion);
            }
            else if (encabezadosinforme.OrdenadoPor == "primeroNoCambios")
            {
                return procesos.OrderBy(x => x.UltimaFechaActuacion);
            }
            else {
                return null;
            }
        }


        //Agrega encabezados al excel
        private bool EncabezadoCampos(DataTable tabla, DataGenerarInforme info)
        {
            if (info.CamposRequeridos.Length>0) {
                foreach (string campo in info.CamposRequeridos)
                {
                    tabla.Columns.Add(campo);
                }
            }
            else
            {
                tabla.Columns.Add("No se seleccionaron campos");
            }
            
            return true;
        }
    }
}
