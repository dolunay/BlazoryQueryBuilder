using BlazorQueryBuilder.Visitors;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace BlazorQueryBuilder.Tests
{

    public class ExpressionVisitorTests
    {
        [Fact]
        public void ChangeBinaryRight()
        {
            ConstantExpression left = Expression.Constant(true);
            ConstantExpression right = Expression.Constant(true);

            BinaryExpression binary = Expression.MakeBinary(ExpressionType.Equal, left, right);

            BinaryExpression newBinary = new ReplaceBinaryRight(binary, Expression.Constant(false)).Replace();

            Assert.IsAssignableFrom<BinaryExpression>(newBinary);
            Assert.True(((ConstantExpression)newBinary.Right).Value.Equals(false));
        }

        [Fact]
        public void ChangeBinaryLeft()
        {
            ConstantExpression left = Expression.Constant(true);
            ConstantExpression right = Expression.Constant(true);

            BinaryExpression binary = Expression.MakeBinary(ExpressionType.Equal, left, right);

            BinaryExpression newBinary = new ReplaceBinaryLeft(binary, Expression.Constant(false)).Replace();

            Assert.IsAssignableFrom<BinaryExpression>(newBinary);
            Assert.True(((ConstantExpression)newBinary.Left).Value.Equals(false));
        }

        [Fact]
        public void ReplaceLambdaBody()
        {
            Expression<Func<Person, bool>> originalLambda = person => person.PersonId == "1";

            BinaryExpression newBody = new ReplaceBinaryType((BinaryExpression)originalLambda.Body, ExpressionType.NotEqual).Replace();

            LambdaExpression newLambda = new ReplaceLambdaBody(originalLambda, newBody).Replace();

            Assert.Equal(newLambda.Body, newBody);
        }

        [Fact]
        public void ChangeBinary()
        {
            BinaryExpression binary1 = Expression.MakeBinary(
                ExpressionType.Equal,
                Expression.Constant(1),
                Expression.Constant(1));

            BinaryExpression binary2 = Expression.MakeBinary(
                ExpressionType.Equal,
                Expression.Constant(2),
                Expression.Constant(2));

            BinaryExpression newBinary = new ReplaceBinary(binary1, binary2).Replace();

            Assert.Equal(newBinary, binary2);
        }

        [Fact]
        public void ChangeBinaryToMethodCall()
        {
            // person.PersonId == "1"
            BinaryExpression personIdEqualsOne =
                Expression.MakeBinary(
                    ExpressionType.Equal,
                Expression.MakeMemberAccess(
                        Expression.Parameter(typeof(Person), "person"),
                        typeof(Person).GetProperty(nameof(Person.PersonId))),
                    Expression.Constant("1"));

            // person.PersonId
            Expression personId = personIdEqualsOne.Left;

            // person.Addresses
            MemberExpression personAddresses = new ChangePropertyAccess(
                    typeof(Person),
                    personId,
                    nameof(Person.Addresses))
                .Change();

            // Select method
            MethodInfo selectMethod = EnumerableMethodInfo.Select<Address, int>();

            // address
            ParameterExpression addressParam = Expression.Parameter(typeof(Address), "address");

            // address.AddressId
            MemberExpression memberAccess = Expression.MakeMemberAccess(
                addressParam,
                typeof(Address).GetProperty(nameof(Address.AddressId)));

            // address => address.AddressId
            LambdaExpression lambda = Expression.Lambda(
                memberAccess,
                addressParam
            );

            // person.Addresses.Select(address => address.AddressId)
            MethodCallExpression selectAddresses = Expression.Call(selectMethod, new List<Expression> { personAddresses, lambda });

            Assert.NotNull(selectAddresses);
        }



    }
}
