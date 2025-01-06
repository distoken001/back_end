using AutoMapper;

namespace CommonLibrary.Common.Common
{
    public class AutoMapperHelper
    {
        /// <summary>
        /// 分配赋值
        /// </summary>
        /// <typeparam name="TSource">数据来源类</typeparam>
        /// <typeparam name="TDestination">目标类</typeparam>
        /// <param name="source">数据来源值</param>
        /// <returns></returns>
        public static TDestination AssignmentMap<TSource, TDestination>(TSource source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });
            var mapper = config.CreateMapper();
            return mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// 分配赋值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="sourceMemberNamingConvention">数据源的成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <param name="destinationMemberNamingConvention">目标成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <returns></returns>
        public static TDestination AssignmentMap<TSource, TDestination>(TSource source, INamingConvention sourceMemberNamingConvention = null, INamingConvention destinationMemberNamingConvention = null)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
                if (sourceMemberNamingConvention != null)
                {
                    cfg.SourceMemberNamingConvention = sourceMemberNamingConvention;
                }
                if (destinationMemberNamingConvention != null)
                {
                    cfg.DestinationMemberNamingConvention = destinationMemberNamingConvention;
                }
            });
            var mapper = config.CreateMapper();
            return mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// 分配赋值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="sourceMemberNamingConvention">数据源的成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <param name="destinationMemberNamingConvention">目标成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <returns></returns>
        public static List<TDestination> AssignmentMap<TSource, TDestination>(List<TSource> source, INamingConvention sourceMemberNamingConvention = null, INamingConvention destinationMemberNamingConvention = null)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
                if (sourceMemberNamingConvention != null)
                {
                    cfg.SourceMemberNamingConvention = sourceMemberNamingConvention;
                }
                if (destinationMemberNamingConvention != null)
                {
                    cfg.DestinationMemberNamingConvention = destinationMemberNamingConvention;
                }
            });
            var mapper = config.CreateMapper();
            return mapper.Map<List<TSource>, List<TDestination>>(source);
        }

        /// <summary>
        /// 数据库实体转换对应数据传输模型
        /// 规则：
        /// 数据库实体类：数据库实体小写并包含下划线
        /// 传输模型类：帕斯卡命名规则（每个单词的首字母大写）
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="sourceMemberNamingConvention">数据源的成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <param name="destinationMemberNamingConvention">目标成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <returns></returns>
        public static TDestination MapDbEntityToDTO<TSource, TDestination>(TSource source, INamingConvention sourceMemberNamingConvention = null, INamingConvention destinationMemberNamingConvention = null)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
                if (sourceMemberNamingConvention != null)
                {
                    cfg.SourceMemberNamingConvention = sourceMemberNamingConvention;
                }
                else
                {
                    cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
                }
                if (destinationMemberNamingConvention != null)
                {
                    cfg.DestinationMemberNamingConvention = destinationMemberNamingConvention;
                }
                else
                {
                    cfg.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
                }
            });
            var mapper = config.CreateMapper();
            return mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// 数据库实体转换对应数据传输模型
        /// 规则：
        /// 数据库实体类：数据库实体小写并包含下划线
        /// 传输模型类：帕斯卡命名规则（每个单词的首字母大写）
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="sourceMemberNamingConvention">数据源的成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <param name="destinationMemberNamingConvention">目标成员变量的命名规则
        /// new LowerUnderscoreNamingConvention() 小写并包含下划线
        /// new PascalCaseNamingConvention() 帕斯卡命名规则（每个单词的首字母大写）
        /// </param>
        /// <returns></returns>
        public static List<TDestination> MapDbEntityToDTO<TSource, TDestination>(List<TSource> source, INamingConvention sourceMemberNamingConvention = null, INamingConvention destinationMemberNamingConvention = null)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
                if (sourceMemberNamingConvention != null)
                {
                    cfg.SourceMemberNamingConvention = sourceMemberNamingConvention;
                }
                else
                {
                    cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
                }
                if (destinationMemberNamingConvention != null)
                {
                    cfg.DestinationMemberNamingConvention = destinationMemberNamingConvention;
                }
                else
                {
                    cfg.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
                }
            });
            var mapper = config.CreateMapper();
            return mapper.Map<List<TSource>, List<TDestination>>(source);
        }
    }
}