﻿using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EDennis.Samples.ColorApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class RgbController : CrudController<ColorContext, Rgb> {

        public RgbController(DbContextProvider<ColorContext> provider, ILogger<QueryController<ColorContext, Rgb>> logger) 
            : base(provider, logger) {
            logger.LogTrace("RgbController entered.");
        }

        public override IQueryable<Rgb> Find(string pathParameter) {
            return _dbContext.Rgb.Where(r=>r.Id == int.Parse(pathParameter));
        }


    }
}
