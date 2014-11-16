using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service;
using alltheairgeadmobileService.DataObjects;
using alltheairgeadmobileService.Models;

namespace alltheairgeadmobileService
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));
            config.SetIsHosted(true);

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            
            Database.SetInitializer(new alltheairgeadmobileInitializer());

            AutoMapper.Mapper.Initialize(cfg =>
            {
                // Mapping from database type to client type
                // Map ExpenseId from database to Id for EntityData
                cfg.CreateMap<Expense, ExpenseDto>()
                    .ForMember(dst => dst.Id, map => map.MapFrom(src => MySqlFuncs.LTRIM(MySqlFuncs.StringConvert(src.ExpenseId))));
                // Mapping from client type to database type
                // Map Id from EntityData to ExpenseId for database
                cfg.CreateMap<ExpenseDto, Expense>()
                    .ForMember(dst => dst.ExpenseId, map => map.MapFrom(src => MySqlFuncs.LongParse(src.Id)));
                // Map to minimum date for smalldatetime SQL type
                cfg.CreateMap<ExpenseDto, Expense>()
                    .ForMember(dst => dst.Time, map => map.MapFrom(src => new DateTime(1900, 1, 1) + ((DateTime)src.Time).TimeOfDay));

            });
        }
    }

    public class alltheairgeadmobileInitializer : ClearDatabaseSchemaIfModelChanges<alltheairgeadmobileContext>
    {
    }
}

