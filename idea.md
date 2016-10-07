# Idea {{ idea.Number }}: {{ idea.Title }} #

## Submitted by {{ idea.Submitter }} on {{ idea.Submitted }}

## {{ idea.Votes }} votes

{{ idea.Text }}

{% for comment in idea.Comments %}
## Comment by {{ comment.Submitter }} on {{ comment.Submitted }}

{{ comment.Content }}
{% endfor %}