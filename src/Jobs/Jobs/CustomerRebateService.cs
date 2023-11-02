//using CommonLibrary.Common.Common;
//using CommonLibrary.DbContext;
//using CommonLibrary.Model.DataEntityModel;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Jobs.Jobs
//{
//    /// <summary>
//    /// 每日凌晨产生前一天的邀请人返佣明细
//    /// </summary>
//    [DisallowConcurrentExecution]
//    public class CustomerRebateService : BaseJob
//    {
//        private readonly ILogger<CustomerRebateService> _logger;
//        private readonly MySqlMasterDbContext _masterDbContext;

//        public CustomerRebateService(ILogger<CustomerRebateService> logger, MySqlMasterDbContext masterDbContext)
//        {
//            _logger = logger;
//            _masterDbContext = masterDbContext;
//        }

//        public override Task ExecuteMethod(IJobExecutionContext context)
//        {
//            return Task.Run(async () =>
//            {
//                try
//                {
//                    _logger.LogDebug($"CustomerRebateService task start {DateTime.Now}");
//                    var date = DateTime.Now.AddDays(-1);
//                    var yesStartDate = new DateTime(date.Date.Year, date.Date.Month, date.Date.Day, 0, 0, 0, 0);

//                    if (_masterDbContext.inviter_rebates.Any(p => p.date == yesStartDate))
//                    {
//                        _logger.LogError($"{yesStartDate}已产生过数据");
//                        return;
//                    }

//                    var yesEndDate = yesStartDate.AddDays(1);
//                    //获取所有邀请人
//                    var inAddresses = await _masterDbContext.users.AsNoTracking().Where(p => p.parent_address != null).Select(p => p.parent_address).Distinct().ToListAsync();
//                    var inEntities = _masterDbContext.users.AsNoTracking().Where(p => inAddresses.Contains(p.address));
//                    //获取所有被邀请用户
//                    var beinQueries = _masterDbContext.users.AsNoTracking().Where(p => p.parent_address != null);
//                    if (beinQueries.Count() > 0)
//                    {
//                        //被邀请人所有地址
//                        var beinAddresses = beinQueries.Select(p => p.address).ToList();

//                        //查出昨日被邀请人为买家所有交易完成的订单
//                        var ordersEntities = _masterDbContext.orders.AsNoTracking().Where(p => p.status == OrderStatus.Completed && p.create_time >= yesStartDate && p.create_time < yesEndDate && beinAddresses.Contains(p.buyer));


//                        var irEntities = await (from order in ordersEntities
//                                                join t in _masterDbContext.chain_tokens.AsNoTracking()
//                                                on new { token = order.token, cid = order.chain_id }
//                                                equals new { token = t.token_address, cid = t.chain_id }
//                                                select new inviter_rebates()
//                                                {
//                                                    date = yesStartDate,
//                                                    order_id = order.id,
//                                                    be_inviter_address = order.buyer,
//                                                    create_time = DateTime.Now,
//                                                    creater = "定时任务",
//                                                    token_name = t.token_name,
//                                                    is_rebate = 0,
//                                                    token_address = order.token,
//                                                    chain_id = order.chain_id,
//                                                    amount = order.price / order.decimals * order.amount
//                                                }).ToListAsync();

//                        //查出昨日被邀请人为卖家所有交易完成的订单
//                        var ordersSellerEntities = _masterDbContext.orders.AsNoTracking().Where(p => p.status == OrderStatus.Completed && p.create_time >= yesStartDate && p.create_time < yesEndDate && beinAddresses.Contains(p.seller));


//                        var irSellerEntities = await (from order in ordersSellerEntities
//                                                      join t in _masterDbContext.chain_tokens.AsNoTracking()
//                                                      on new { token = order.token, cid = order.chain_id }
//                                                      equals new { token = t.token_address, cid = t.chain_id }
//                                                      select new inviter_rebates()
//                                                      {
//                                                          date = yesStartDate,
//                                                          order_id = order.id,
//                                                          be_inviter_address = order.seller,
//                                                          create_time = DateTime.Now,
//                                                          creater = "定时任务",
//                                                          token_name = t.token_name,
//                                                          is_rebate = 0,
//                                                          token_address = order.token,
//                                                          chain_id = order.chain_id,
//                                                          amount = order.price / order.decimals * order.amount
//                                                      }).ToListAsync();

//                        var resultEntities = new List<inviter_rebates>();
//                        //合并买家或卖家为被邀请人的明细
//                        resultEntities.AddRange(irSellerEntities);
//                        resultEntities.AddRange(irEntities);

//                        //为邀请人赋值
//                        foreach (var item in resultEntities)
//                        {
//                            item.inviter_address = beinQueries.FirstOrDefault(p => p.address == item.be_inviter_address)?.parent_address;
//                            //获取邀请人的费率
//                            var _user = await inEntities.FirstOrDefaultAsync(p => p.address == item.inviter_address);
//                            item.amount = item.amount * _user.rate;
//                        }

//                        await _masterDbContext.inviter_rebates.AddRangeAsync(resultEntities);
//                        await _masterDbContext.SaveChangesAsync();
//                        _logger.LogError($"本次共生成{resultEntities.Count}条明细");
//                    }

//                    _logger.LogDebug($"CustomerRebateService task end {DateTime.Now}");
//                }
//                catch (Exception ex)
//                {

//                    _logger.LogDebug($"CustomerRebateService task 异常 {ex.InnerException.GetBaseException().Message}");
//                }
//            });
//        }
//    }
//}
