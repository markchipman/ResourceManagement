﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResourceManagement.Entities;
using ResourceManagement.Helpers;

namespace ResourceManagement.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest)
        {
            var city = GetCity(cityId, false);
            city.PointsOfInterest.Add(pointOfInterest);
        }

        public bool CityExists(int cityId)
        {
            return _context.Cities.Any(c => c.Id == cityId);
        }        

        public IEnumerable<City> GetCities()
        {
            return _context.Cities.OrderBy(c => c.Name).ToList();
        }

        public City GetCity(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
            {
                return _context.Cities.Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId).FirstOrDefault();
            }

            return _context.Cities.Where(c => c.Id == cityId).FirstOrDefault();
        }

        public PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId)
        {
            return _context.PointsOfInterest
               .Where(p => p.CityId == cityId && p.Id == pointOfInterestId).FirstOrDefault();
        }

        public IEnumerable<PointOfInterest> GetPointsOfInterestForCity(int cityId)
        {
            return _context.PointsOfInterest
                           .Where(p => p.CityId == cityId).ToList();
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public PagedList<City> GetCitiesWithPaging(CityResourceParameters cityResourceParameters)
        {
            var collectionBeforePaging = _context.Cities
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Description).AsQueryable();

            if (!string.IsNullOrWhiteSpace(cityResourceParameters.Genre))
            {
                var genreForWhereClause = cityResourceParameters.Genre.Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging.Where(c => c.Name.ToLowerInvariant() == genreForWhereClause);
            }

            if (!string.IsNullOrWhiteSpace(cityResourceParameters.SearchQuery))
            {
                var searchQuery = cityResourceParameters.SearchQuery.Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging.Where(c => c.Name.ToLowerInvariant().Contains(searchQuery)
                || c.Description.ToLowerInvariant().Contains(searchQuery));
            }            

            return PagedList<City>.Create(collectionBeforePaging, cityResourceParameters.PageNumber, cityResourceParameters.PageSize);            
        }
    }
}
