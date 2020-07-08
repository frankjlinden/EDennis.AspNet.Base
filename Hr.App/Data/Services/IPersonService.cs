﻿using Hr.App.Data.Models;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Hr.App.Data.Services {
    public interface IPersonService : IGetPage {
        Task<Person> GetAsync(int id);
        Task<Person> CreateAsync(Person person);
        Task<Person> UpdateAsync(Person person);
        Task DeleteAsync(int id);
    }
}