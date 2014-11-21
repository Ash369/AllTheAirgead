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
            
            Database.SetInitializer(new alltheairgeadInitializer());

            // Configure automapper to map class elements between EntityFramework model and model used in database
            AutoMapper.Mapper.Initialize(cfg =>
            {
                /*********************************************/
                /* Mapping from database type to client type */
                /*********************************************/
                // Map ExpenseId from database to Id for EntityData
                cfg.CreateMap<Expense, ExpenseDto>()
                    .ForMember(dst => dst.Id, map => map.MapFrom(src => MySqlFuncs.LTRIM(MySqlFuncs.StringConvert(src.ExpenseId))));
                // Map priority stored as a short in the database to byte in the mobile service
                cfg.CreateMap<Expense, ExpenseDto>()
                    .ForMember(dst => dst.Priority, map => map.MapFrom(src => (short)src.Priority));
                // Map category name used in database to id string from EntityFramework
                cfg.CreateMap<Catagory, CategoryDto>()
                    .ForMember(dst => dst.Id, map => map.MapFrom(src => src.CategoryName));

                /*********************************************/
                /* Mapping from client type to database type */
                /*********************************************/
                // Map Id from EntityData to ExpenseId for database
                cfg.CreateMap<ExpenseDto, Expense>()
                    .ForMember(dst => dst.ExpenseId, map => map.MapFrom(src => MySqlFuncs.LongParse(src.Id)));
                // Map to minimum date for smalldatetime SQL type
                cfg.CreateMap<ExpenseDto, Expense>()
                    .ForMember(dst => dst.Time, map => map.MapFrom(src => new DateTime(1900, 1, 1) + ((DateTime)src.Time).TimeOfDay));
                // Map priority stored as a byte in the mobile service to a short in the database
                cfg.CreateMap<ExpenseDto, Expense>()
                    .ForMember(dst => dst.Priority, map => map.MapFrom(src => (Byte)src.Priority));
                // Map category id string from EntityFramework to category name used in database
                cfg.CreateMap<CategoryDto, Catagory>()
                    .ForMember(dst => dst.CategoryName, map => map.MapFrom(src => src.Id));

            });
        }
    }

    public class alltheairgeadInitializer : ClearDatabaseSchemaIfModelChanges<alltheairgeadContext>
    {
    }
}

