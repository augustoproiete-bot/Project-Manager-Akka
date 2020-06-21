using System.Windows.Data;
using Akka.Cluster;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.Wpf.Converter;

namespace Tauron.Application.ServiceManager.Converter
{
    public sealed class MemberStatusConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create()
        {
            var loc = LocLocalizer.Inst.MemberStatus;
            return CreateStringConverter<MemberStatus>(s =>
                                                       {
                                                           return s switch
                                                           {
                                                               MemberStatus.Joining => loc.Joining,
                                                               MemberStatus.Up => loc.Up,
                                                               MemberStatus.Leaving => loc.Leaving,
                                                               MemberStatus.Exiting => loc.Exiting,
                                                               MemberStatus.Down => loc.Down,
                                                               MemberStatus.WeaklyUp => loc.WeaklyUp,
                                                               _ => ""
                                                           };
                                                       });
        }
    }
}