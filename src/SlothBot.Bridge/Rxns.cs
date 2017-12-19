using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace SlothBot.Bridge
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string IfIsNullOrWhiteSpace(this string str, string nullValue)
        {
            return str.IsNullOrWhiteSpace() ? nullValue : str;
        }
    }


    /// <summary>
    /// This code is from the Rxns library, but it doesnt support .netStandard yet so this a copy/paste
    /// </summary>
    public static class RxObservable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toExecute"></param>
        /// <param name="onError">Override the default error handling behaviour, which just passes through the exception on the Observables OnError channel</param>
        /// <param name="allowNull">By default, the observable sequence will onComplete if a null is returned. This flag will onNext the null before oncompleting it</param>
        /// <returns></returns>
        public static IObservable<T> Create<T>(Func<T> toExecute, Func<Exception, T> onError = null,
            bool allowNull = false)
        {
            return Observable.Create<T>(o =>
            {
                try
                {
                    var result = toExecute();

                    if (result != null || allowNull)
                        o.OnNext(result);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        o.OnNext(onError(e));
                    else
                        o.OnError(e);
                }
                finally
                {
                    o.OnCompleted();
                }

                return Disposable.Empty;
            });
        }

        public static IObservable<T> Create<T>(Action toExecute, Func<Exception, T> onError = null)
        {
            return Observable.Create<T>(o =>
            {
                try
                {
                    toExecute();
                }
                catch (Exception e)
                {
                    if (onError != null)
                        o.OnNext(onError(e));
                    else
                        o.OnError(e);
                }
                finally
                {
                    o.OnCompleted();
                }

                return Disposable.Empty;
            });
        }

        public static IObservable<Unit> Create(Action toExecute, Func<Exception, Unit> onError = null)
        {
            return Observable.Create<Unit>(o =>
            {
                try
                {
                    toExecute();

                    o.OnNext(new Unit());
                }
                catch (Exception e)
                {
                    if (onError != null)
                        o.OnNext(onError(e));
                    else
                        o.OnError(e);
                }
                finally
                {
                    o.OnCompleted();
                }

                return Disposable.Empty;
            });
        }

        public static IObservable<T> Create<T>(Func<IObservable<T>> toExecute)
        {
            return Observable.Create<T>(o =>
            {
                try
                {
                    var result = toExecute();
                    if (result == null)
                    {
                        o.OnCompleted();
                        return Disposable.Empty;
                    }

                    return result.Subscribe(o);
                }
                catch (Exception e)
                {
                    o.OnError(e);
                    return Disposable.Empty;
                }
            });
        }

        public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> toExecute)
        {
            return Observable.Create<T>(o =>
            {
                try
                {
                    return toExecute(o);
                }
                catch (Exception e)
                {
                    o.OnError(e);
                    return Disposable.Empty;
                }
            });
        }

        public static IObservable<T> DfrCreate<T>(Func<T> toExecute, Func<Exception, T> onError = null,
            bool allowNull = false)
        {
            return Observable.Defer(() => Create(toExecute, onError, allowNull));
        }

        public static IObservable<T> DfrCreate<T>(Func<IObservable<T>> toExecute)
        {
            return Observable.Defer(() => Create(toExecute));
        }

        public static IObservable<T> DfrCreate<T>(Func<IObserver<T>, IDisposable> toExecute)
        {
            return Observable.Defer(() => Create(toExecute));
        }
        
        public class DisposableAction : IDisposable
        {
            private readonly Action _onDispose;

            public DisposableAction(Action onDispose)
            {
                _onDispose = onDispose;
            }
            public void Dispose()
            {
                _onDispose?.Invoke();
            }
        }
        public static string ToStringEach<T>(this IEnumerable<T> list, string delimiter = ",")
        {
            return String.Join(delimiter, list);
        }

        /// <summary>
        /// A reliable version of finally that doesnt matter how the user handles the resulting
        /// obsrevable, it will always be called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="finallyAction"></param>
        /// <returns></returns>
        public static IObservable<T> FinallyR<T>(this IObservable<T> source, Action finallyAction)
        {
            return Observable.Create<T>(o =>
            {
                var finallyOnce = Disposable.Create(finallyAction);
                var subscription = source.Subscribe(
                    o.OnNext,
                    ex =>
                    {
                        try
                        {
                            o.OnError(ex);
                        }
                        finally
                        {
                            finallyOnce.Dispose();
                        }
                    },
                    () =>
                    {
                        try
                        {
                            o.OnCompleted();
                        }
                        finally
                        {
                            finallyOnce.Dispose();
                        }
                    });

                return new CompositeDisposable(subscription, finallyOnce);

            });
        }
        /// <summary>
        /// The same as Any, but works with nulls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns>If the list has any elements</returns>
        public static bool AnyItems<T>(this IEnumerable<T> list)
        {
            return list != null && list.Any();
        }
    }
}
