using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace frontend
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            /* Las páginas de listado de un espacio van bajo e/{espacio}/...
               y son las mismas páginas físicas del sitio público: la capa de
               servicios lee el espacio de la ruta (Espacios.SlugActual) y
               acota cada consulta. Sin prefijo, la misma página muestra la
               plataforma.

               Van antes de FriendlyUrls porque las rutas se prueban en orden
               y la de FriendlyUrls atrapa todo lo que se parezca a una página.

               Las fichas (Campana?c=, Candidato?id=, Partido?id=,
               Propuesta?id=) no llevan prefijo a propósito: el objeto ya sabe
               su espacio y la página lo declara con Espacios.Fijar, así un
               enlace compartido se dibuja siempre dentro de su espacio. */

            routes.MapPageRoute("EspacioInicio",     "e/{espacio}",            "~/Default.aspx");
            routes.MapPageRoute("EspacioCampanas",   "e/{espacio}/Campanas",   "~/Campanas.aspx");
            routes.MapPageRoute("EspacioCandidatos", "e/{espacio}/Candidatos", "~/Candidatos.aspx");
            routes.MapPageRoute("EspacioPartidos",   "e/{espacio}/Partidos",   "~/Partidos.aspx");
            routes.MapPageRoute("EspacioPropuestas", "e/{espacio}/Propuestas", "~/Propuestas.aspx");
            routes.MapPageRoute("EspacioAnalitica",  "e/{espacio}/Analitica",  "~/Analitica.aspx");

            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;
            routes.EnableFriendlyUrls(settings);
        }
    }
}
