﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Octokit.Internal;
using Octokit.Reactive;
using System.Reactive.Threading.Tasks;
using Xunit;

namespace Octokit.Tests.Reactive
{
    public class ObservablePullRequestReviewCommentsClientTests
    {
        public class TheCtor
        {
            [Fact]
            public void EnsuresNonNullArguments()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new ObservablePullRequestReviewCommentsClient(null));
            }
        }

        static IResponse CreateResponseWithApiInfo(IDictionary<string, Uri> links)
        {
            var response = Substitute.For<IResponse>();
            response.ApiInfo.Returns(new ApiInfo(links, new List<string>(), new List<string>(), "etag", new RateLimit(new Dictionary<string, string>())));
            return response;
        }

        public class TheGetForPullRequestMethod
        {
            [Fact]
            public void RequestsCorrectUrl()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                client.GetAll("fake", "repo", 1);

                gitHubClient.Received().PullRequest.Comment.GetAll("fake", "repo", 1);
            }

            [Fact]
            public void RequestsCorrectUrlWithApiOptions()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var options = new ApiOptions
                {
                    PageCount = 1,
                    PageSize = 1,
                    StartPage = 1
                };

                client.GetAll("fake", "repo", 1, options);

                gitHubClient.Received().PullRequest.Comment.GetAll("fake", "repo", 1, options);
            }

            [Fact]
            public async Task RequestsCorrectUrlMulti()
            {
                var firstPageUrl = new Uri("repos/fakeOwner/fakeRepoName/pulls/7/comments", UriKind.Relative);
                var secondPageUrl = new Uri("https://example.com/page/2");
                var firstPageLinks = new Dictionary<string, Uri> { { "next", secondPageUrl } };
                var firstPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    CreateResponseWithApiInfo(firstPageLinks),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(1),
                        new PullRequestReviewComment(2),
                        new PullRequestReviewComment(3)
                    });
                var thirdPageUrl = new Uri("https://example.com/page/3");
                var secondPageLinks = new Dictionary<string, Uri> { { "next", thirdPageUrl } };
                var secondPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    CreateResponseWithApiInfo(secondPageLinks),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(4),
                        new PullRequestReviewComment(5),
                        new PullRequestReviewComment(6)
                    }
                );
                var lastPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    new Response(),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(7)
                    }
                );

                var gitHubClient = Substitute.For<IGitHubClient>();
                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(firstPageUrl, Args.EmptyDictionary, null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => firstPageResponse));
                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(secondPageUrl, Args.EmptyDictionary, null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => secondPageResponse));
                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(thirdPageUrl, Args.EmptyDictionary, null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => lastPageResponse));

                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var results = await client.GetAll("fakeOwner", "fakeRepoName", 7).ToArray();

                Assert.Equal(7, results.Length);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(firstPageUrl, Args.EmptyDictionary, null);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(secondPageUrl, Args.EmptyDictionary, null);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(thirdPageUrl, Args.EmptyDictionary, null);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                Assert.Throws<ArgumentNullException>(() => client.GetAll(null, "name", 1));
                Assert.Throws<ArgumentNullException>(() => client.GetAll("owner", null, 1));

                Assert.Throws<ArgumentNullException>(() => client.GetAll(null, "name", 1, ApiOptions.None));
                Assert.Throws<ArgumentNullException>(() => client.GetAll("owner", null, 1, ApiOptions.None));
                Assert.Throws<ArgumentNullException>(() => client.GetAll("owner", "name", 1, null));

                Assert.Throws<ArgumentException>(() => client.GetAll("", "name", 1));
                Assert.Throws<ArgumentException>(() => client.GetAll("owner", "", 1));

                Assert.Throws<ArgumentException>(() => client.GetAll("", "name", 1, ApiOptions.None));
                Assert.Throws<ArgumentException>(() => client.GetAll("owner", "", 1, ApiOptions.None));
            }
        }

        public class TheGetForRepositoryMethod
        {
            [Fact]
            public void RequestsCorrectUrl()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var request = new PullRequestReviewCommentRequest
                {
                    Direction = SortDirection.Descending,
                    Since = new DateTimeOffset(2013, 11, 15, 11, 43, 01, 00, new TimeSpan()),
                    Sort = PullRequestReviewCommentSort.Updated
                };

                client.GetAllForRepository("fake", "repo", request);

                gitHubClient.Received().PullRequest.Comment.GetAllForRepository("fake", "repo", request);
            }

            [Fact]
            public void RequestsCorrectUrlWithApiOptions()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var request = new PullRequestReviewCommentRequest
                {
                    Direction = SortDirection.Descending,
                    Since = new DateTimeOffset(2013, 11, 15, 11, 43, 01, 00, new TimeSpan()),
                    Sort = PullRequestReviewCommentSort.Updated
                };

                var options = new ApiOptions
                {
                    PageCount = 1,
                    PageSize = 1,
                    StartPage = 1
                };

                client.GetAllForRepository("fake", "repo", request, options);

                gitHubClient.Received().PullRequest.Comment.GetAllForRepository("fake", "repo", request, options);
            }

            [Fact]
            public async Task RequestsCorrectUrlMulti()
            {
                var firstPageUrl = new Uri("repos/fakeOwner/fakeRepoName/pulls/comments", UriKind.Relative);
                var secondPageUrl = new Uri("https://example.com/page/2");
                var firstPageLinks = new Dictionary<string, Uri> { { "next", secondPageUrl } };
                var firstPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    CreateResponseWithApiInfo(firstPageLinks),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(1),
                        new PullRequestReviewComment(2),
                        new PullRequestReviewComment(3)
                    }
                );
                var thirdPageUrl = new Uri("https://example.com/page/3");
                var secondPageLinks = new Dictionary<string, Uri> { { "next", thirdPageUrl } };
                var secondPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    CreateResponseWithApiInfo(secondPageLinks),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(4),
                        new PullRequestReviewComment(5),
                        new PullRequestReviewComment(6)
                    });
                var lastPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    new Response(),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(7),
                        new PullRequestReviewComment(8)
                    }
                );

                var gitHubClient = Substitute.For<IGitHubClient>();

                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(firstPageUrl,
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 3
                        && d["direction"] == "desc"
                        && d["since"] == "2013-11-15T11:43:01Z"
                        && d["sort"] == "updated"), null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => firstPageResponse));
                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(secondPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 3
                        && d["direction"] == "desc"
                        && d["since"] == "2013-11-15T11:43:01Z"
                        && d["sort"] == "updated"), null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => secondPageResponse));
                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(thirdPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 3
                        && d["direction"] == "desc"
                        && d["since"] == "2013-11-15T11:43:01Z"
                        && d["sort"] == "updated"), null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => lastPageResponse));

                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var request = new PullRequestReviewCommentRequest
                {
                    Direction = SortDirection.Descending,
                    Since = new DateTimeOffset(2013, 11, 15, 11, 43, 01, 00, new TimeSpan()),
                    Sort = PullRequestReviewCommentSort.Updated
                };

                var results = await client.GetAllForRepository("fakeOwner", "fakeRepoName", request).ToArray();

                Assert.Equal(8, results.Length);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(firstPageUrl,
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 3
                        && d["direction"] == "desc"
                        && d["since"] == "2013-11-15T11:43:01Z"
                        && d["sort"] == "updated"), null);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(secondPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 3
                        && d["direction"] == "desc"
                        && d["since"] == "2013-11-15T11:43:01Z"
                        && d["sort"] == "updated"), null);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(thirdPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 3
                        && d["direction"] == "desc"
                        && d["since"] == "2013-11-15T11:43:01Z"
                        && d["sort"] == "updated"), null);
            }

            [Fact]
            public void RequestsCorrectUrlWithoutSelectedSortingArguments()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                client.GetAllForRepository("fake", "repo");

                gitHubClient.Received().PullRequest.Comment.GetAllForRepository("fake", "repo");
            }

            [Fact]
            public void RequestsCorrectUrlWithoutSelectedSortingArgumentsWithApiOptions()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var options = new ApiOptions
                {
                    PageCount = 1,
                    PageSize = 1,
                    StartPage = 1
                };

                client.GetAllForRepository("fake", "repo", options);

                gitHubClient.Received().PullRequest.Comment.GetAllForRepository("fake", "repo", options);
            }

            [Fact]
            public async Task RequestsCorrectUrlWithoutSelectedSortingArgumentsMulti()
            {
                var firstPageUrl = new Uri("repos/fakeOwner/fakeRepoName/pulls/comments", UriKind.Relative);
                var secondPageUrl = new Uri("https://example.com/page/2");
                var firstPageLinks = new Dictionary<string, Uri> { { "next", secondPageUrl } };
                var firstPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    CreateResponseWithApiInfo(firstPageLinks),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(1),
                        new PullRequestReviewComment(2),
                        new PullRequestReviewComment(3)
                    }
                );
                var thirdPageUrl = new Uri("https://example.com/page/3");
                var secondPageLinks = new Dictionary<string, Uri> { { "next", thirdPageUrl } };
                var secondPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    CreateResponseWithApiInfo(secondPageLinks),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(4),
                        new PullRequestReviewComment(5),
                        new PullRequestReviewComment(6)
                    });
                var lastPageResponse = new ApiResponse<List<PullRequestReviewComment>>
                (
                    new Response(),
                    new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment(7),
                        new PullRequestReviewComment(8)
                    }
                );

                var gitHubClient = Substitute.For<IGitHubClient>();

                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(firstPageUrl,
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 2
                        && d["direction"] == "asc"
                        && d["sort"] == "created"), null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => firstPageResponse));
                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(secondPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 2
                        && d["direction"] == "asc"
                        && d["sort"] == "created"), null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => secondPageResponse));
                gitHubClient.Connection.Get<List<PullRequestReviewComment>>(thirdPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 2
                        && d["direction"] == "asc"
                        && d["sort"] == "created"), null)
                    .Returns(Task.Factory.StartNew<IApiResponse<List<PullRequestReviewComment>>>(() => lastPageResponse));

                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var results = await client.GetAllForRepository("fakeOwner", "fakeRepoName").ToArray();

                Assert.Equal(8, results.Length);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(firstPageUrl,
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 2
                        && d["direction"] == "asc"
                        && d["sort"] == "created"), null);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(secondPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 2
                        && d["direction"] == "asc"
                        && d["sort"] == "created"), null);
                gitHubClient.Connection.Received(1).Get<List<PullRequestReviewComment>>(thirdPageUrl, Arg.Is<Dictionary<string, string>>(d => d.Count == 2
                        && d["direction"] == "asc"
                        && d["sort"] == "created"), null);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var client = new ObservablePullRequestReviewCommentsClient(Substitute.For<IGitHubClient>());

                var request = new PullRequestReviewCommentRequest();

                Assert.Throws<ArgumentNullException>(() => client.GetAllForRepository(null, "name", request));
                Assert.Throws<ArgumentNullException>(() => client.GetAllForRepository("owner", null, request));
                Assert.Throws<ArgumentNullException>(() => client.GetAllForRepository("owner", "name", (PullRequestReviewCommentRequest)null));

                Assert.Throws<ArgumentNullException>(() => client.GetAllForRepository(null, "name", request, ApiOptions.None));
                Assert.Throws<ArgumentNullException>(() => client.GetAllForRepository("owner", null, request, ApiOptions.None));
                Assert.Throws<ArgumentNullException>(() => client.GetAllForRepository("owner", "name", null, ApiOptions.None));
                Assert.Throws<ArgumentNullException>(() => client.GetAllForRepository("owner", "name", request, null));

                Assert.Throws<ArgumentException>(() => client.GetAllForRepository("", "name", request));
                Assert.Throws<ArgumentException>(() => client.GetAllForRepository("owner", "", request));
                Assert.Throws<ArgumentException>(() => client.GetAllForRepository("", "name", request, ApiOptions.None));
                Assert.Throws<ArgumentException>(() => client.GetAllForRepository("owner", "", request, ApiOptions.None));
            }
        }

        public class TheGetCommentMethod
        {
            [Fact]
            public void GetsFromClientPullRequestComment()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                client.GetComment("fakeOwner", "fakeRepoName", 53);

                gitHubClient.PullRequest.Comment.Received().GetComment("fakeOwner", "fakeRepoName", 53);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var client = new ObservablePullRequestReviewCommentsClient(Substitute.For<IGitHubClient>());

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetComment(null, "name", 1).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetComment("", "name", 1).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetComment("owner", null, 1).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetComment("owner", "", 1).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetComment(null, null, 1).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetComment("", "", 1).ToTask());
            }
        }

        public class TheCreateMethod
        {
            [Fact]
            public void PostsToCorrectUrl()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var comment = new PullRequestReviewCommentCreate("Comment content", "qe3dsdsf6", "file.css", 7);

                client.Create("fakeOwner", "fakeRepoName", 13, comment);

                gitHubClient.PullRequest.Comment.Received().Create("fakeOwner", "fakeRepoName", 13, comment);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                string body = "Comment content";
                string commitId = "qe3dsdsf6";
                string path = "file.css";
                int position = 7;

                var comment = new PullRequestReviewCommentCreate(body, commitId, path, position);

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Create(null, "name", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.Create("", "name", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Create("owner", null, 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.Create("owner", "", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Create("owner", "name", 1, null).ToTask());
            }
        }

        public class TheCreateReplyMethod
        {
            [Fact]
            public void PostsToCorrectUrl()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var comment = new PullRequestReviewCommentReplyCreate("Comment content", 9);

                client.CreateReply("fakeOwner", "fakeRepoName", 13, comment);

                gitHubClient.PullRequest.Comment.Received().CreateReply("fakeOwner", "fakeRepoName", 13, comment);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                string body = "Comment content";
                int inReplyTo = 7;

                var comment = new PullRequestReviewCommentReplyCreate(body, inReplyTo);

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateReply(null, "name", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.CreateReply("", "name", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateReply("owner", null, 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.CreateReply("owner", "", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateReply("owner", "name", 1, null).ToTask());
            }
        }

        public class TheEditMethod
        {
            [Fact]
            public void PostsToCorrectUrl()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var comment = new PullRequestReviewCommentEdit("New comment content");

                client.Edit("fakeOwner", "fakeRepoName", 13, comment);

                gitHubClient.PullRequest.Comment.Received().Edit("fakeOwner", "fakeRepoName", 13, comment);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                var body = "New comment content";

                var comment = new PullRequestReviewCommentEdit(body);

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Edit(null, "name", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.Edit("", "name", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Edit("owner", null, 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.Edit("owner", "", 1, comment).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Edit("owner", "name", 1, null).ToTask());
            }
        }

        public class TheDeleteMethod
        {
            [Fact]
            public void PostsToCorrectUrl()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                client.Delete("fakeOwner", "fakeRepoName", 13);

                gitHubClient.PullRequest.Comment.Received().Delete("fakeOwner", "fakeRepoName", 13);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservablePullRequestReviewCommentsClient(gitHubClient);

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Delete(null, "name", 1).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.Delete("", "name", 1).ToTask());
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.Delete("owner", null, 1).ToTask());
                await Assert.ThrowsAsync<ArgumentException>(() => client.Delete("owner", "", 1).ToTask());
            }
        }
    }
}
