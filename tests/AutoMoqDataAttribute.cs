using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SoliTests.Repositories;

namespace SoliTests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : 
            base(() => {
                var fix = new Fixture().Customize(new AutoMoqCustomization());
                fix.Customize<BindingInfo>(c => c.OmitAutoProperties());
                return fix;
            })
        {}
    }
    
    public class AutoMoqDbDataAttribute : AutoDataAttribute
    {
        public AutoMoqDbDataAttribute()
            :base(() => {
                var fix = new Fixture().Customize(new AutoMoqCustomization());
                var dbConn = MockDatabase.EstablishDatabase();
                fix.Customizations.Add(new DbConnectionArg(dbConn));
                return fix;
            })
        {}
    }

    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] objects)
            : base(new AutoMoqDataAttribute(), objects)
        {}
    }
}
