using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using orch.common;
using orch.core.ef.System.Entities;
using orch.core.model;
using orch.core.model.dto;
using System.Security.Cryptography;

namespace orch.core.ef.System
{
    public class EFSystemDatabase : ISystemDatabase
    {
        private readonly OSystemDbContext _dbContext;
        private readonly IOHost _host;
        private readonly ContentServerConfig _contentServerConfig;

        public EFSystemDatabase(OSystemDbContext dbContext, IOHost host, IOptions<ContentServerConfig> config)
        {
            _dbContext = dbContext;
            _host = host;
            _contentServerConfig = config.Value;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        private  void AssertActiveAccessToken(Guid tokenId, AccessTokenProps? accessToken)
        {
            if (accessToken == null)
                throw new InvalidOperationException($"Access token {tokenId} doesn't exist");
            
            var now = _host.CurrentTime();
            
            if (accessToken.ExpiryTime != null && accessToken.ExpiryTime < now)
                throw new InvalidOperationException($"Access token {tokenId} has expired, it can't be used.");
        }

        public void CreateAccessToken(AccessTokenProps accessToken)
        {
            _dbContext.AccessTokens.Add(new DALAccessToken(accessToken));
            _dbContext.SaveChanges();
        }

        public void CreateFile(ContentFile cf)
        {
            var dalFile = new DALContentFile(cf);
            _dbContext.Files.Add(new DALContentFile(cf));
            _dbContext.SaveChanges();
            _dbContext.Entry(dalFile).State = EntityState.Detached;
        }

        public void DeleteFile(Guid fileId)
        {
            if (!_dbContext.Files.Any(f => f.FileId == fileId))
                throw new FileNotFoundException($"File {fileId} doesn't exist");

            _dbContext.Files.Remove(_dbContext.Files.First(f => f.FileId == fileId));
            _dbContext.SaveChanges();
        }

        public void DeleteAccessToken(params Guid[] accessTokens)
        {
            var now = _host.CurrentTime();
            var tokensToDelete = _dbContext.AccessTokens
                .Where(t => accessTokens.Contains(t.Token))
                .ToList();

            foreach (var token in tokensToDelete)
            {
                AssertActiveAccessToken(token.Token, token);
                token.ExpiryTime = now;
                _dbContext.AccessTokens.Update(token);
            }

            _dbContext.SaveChanges();

            tokensToDelete.ForEach(token => _dbContext.Entry(token).State = EntityState.Detached);
        }

        public AccessToken? GetAccessTokenInfo(Guid tokenId)
        {
            var res = _dbContext.AccessTokens.Where(accessToken => accessToken.Token == tokenId)
                          .Select(accessToken => new AccessToken(accessToken))
                          .FirstOrDefault();
            return res;
        }

        public ContentFile? GetFile(Guid file_id)
        {
            return _dbContext.Files
                .AsNoTracking()
                .Where(file => file.FileId == file_id)
                .Select(file => new ContentFile(file))
                .FirstOrDefault();
        }

        public PagedList<ContentFile> GetFiles(int pageNumber, int pageSize, ContentFileFilter? filter = null)
        {
            var queryable = _dbContext.Files.AsQueryable();

            if (filter != null)
            {
                if (filter.FromCreateTime != null)
                    queryable = queryable.Where(f => f.CreateTime >= filter.FromCreateTime.Value);
            }

            var totalItemCount = queryable.Count();
            var contentFiles = queryable.OrderBy(f => f.CreateTime)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Select(file => new ContentFile(file))
                                    .ToList();

            return new PagedList<ContentFile>
            {
                List = contentFiles,
                Count = totalItemCount
            };
        }

        public AccessToken? PingAccessToken(Guid tokenId)
        {
            var token = _dbContext.AccessTokens
                .AsNoTracking()
                .Where(accessToken => accessToken.Token == tokenId)
                .FirstOrDefault();

            AssertActiveAccessToken(tokenId, token);

            if (token == null)
            {
                return null;
            }

            token.LastUsed = _host.CurrentTime();
            _dbContext.AccessTokens.Update(token);
            _dbContext.SaveChanges();
            _dbContext.Entry(token).State = EntityState.Detached;
            return new AccessToken(token);
        }


        public List<AccessToken> GetTokensByUserId(Guid userId)
        {
            var now = _host.CurrentTime();
            return _dbContext.AccessTokens
                .Where(t => t.UserId == userId && (t.ExpiryTime == null || t.ExpiryTime > now))
                .OrderBy(t => t.CreatedTime)
                .Select(t => new AccessToken(t))
                .ToList();
        }

        public ContentFile SaveFile(string fileName, Stream r, Guid? fileId = null, bool overwrite = false)
        {
            if (r.Length > _contentServerConfig.MaxFileSize)
                throw new ArgumentException($"File size ({r.Length / (1024.0 * 1024.0):F2} MB) exceeds the maximum allowed size ({_contentServerConfig.MaxFileSize / (1024.0 * 1024.0):F2} MB)");

            var item = new ContentFile
            {
                FileId = fileId ?? _host.NextGuid(),
                FileName = fileName,
                MimeType = MimeMapping.MimeUtility.GetMimeMapping(fileName), //TODO: check if we can use f.ContentType
                CreateTime = _host.CurrentTime()
            };

            
            if (string.IsNullOrWhiteSpace(_contentServerConfig.BaseDir))
                throw new InvalidOperationException($"{nameof(_contentServerConfig.BaseDir)} must be configured and non-empty.");
            
            if (!Directory.Exists(_contentServerConfig.BaseDir))
            {
                Directory.CreateDirectory(_contentServerConfig.BaseDir);
            }

            // Create hash function and initialize it
            HashAlgorithm hashFunc = SHA256.Create();
            hashFunc.Initialize();

            var buffer = new byte[4096];

            // Create file path for saving the file
            var filePath = Path.Combine(_contentServerConfig.BaseDir, $"{item.FileId}.content");

            if (File.Exists(filePath) && !overwrite)
                throw new InvalidOperationException($"File '{item.FileId}' already exists on file system");

            // Create file and write stream data to it
            using (var os = File.Create(filePath))
            {
                int len;

                do
                {
                    len = r.Read(buffer, 0, buffer.Length);
                    if (len > 0)
                    {
                        hashFunc.TransformBlock(buffer, 0, len, null, 0);
                        os.Write(buffer, 0, len);
                    }
                } while (len > 0);
            }

            // Finalize hash and dispose of hash function
            hashFunc.TransformFinalBlock(buffer, 0, 0);
            hashFunc.Dispose();

            var transaction = _dbContext.Database.BeginTransaction();
            
            var existingFile = GetFile(item.FileId);

            try
            {
                if (existingFile is not null)
                {
                    if (overwrite)
                    {
                        DeleteFile(existingFile.FileId);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Content index for file '{existingFile.FileId}' already exists");
                    }
                }

                CreateFile(item);

                transaction.Commit();
                
                return item;
            }
            catch
            {
                transaction.Rollback();

                // If creating entry in database fails, delete the saved file and rethrow exception
                try
                {
                    if (existingFile is null)
                        File.Delete(filePath);
                }
                catch (Exception deleteException)
                {
                    throw new IOException("Error trying to rollback file creation", deleteException);
                }
                
                throw;
            }
        }
    }
}