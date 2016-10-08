# Idea {{ idea.Number }}: {{ idea.Title }} #

## Status : {{ idea.Status }}

## Submitted by {{ idea.Submitter }} on {{ idea.Submitted }}

## {{ idea.Votes }} votes

{{ idea.Text }}

{% if idea.Response %}
## Response by fslang-admin on {{ idea.Response.Responded }}

{{ idea.Response.Text }}
{% endif %}

{% for comment in idea.Comments %}
## Comment by {{ comment.Submitter }} on {{ comment.Submitted }}

{{ comment.Content }}
{% endfor %}
