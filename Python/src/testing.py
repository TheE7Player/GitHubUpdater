import githubupdater
from Classes.Repo import Repo
from githubupdater import get_updates


def test_get_release():
    test = Repo('CSGO-Event-Viewer', 'TheE7Player/CSGO-Event-Viewer',
                'https://github.com/TheE7Player/CSGO-Event-Viewer',
                'https://api.github.com/repos/TheE7Player/CSGO-Event-Viewer',
                'A Java project to allow users to see what events you can use with logic_eventlistener', 'Java',
                '2019-11-16T19:00:17Z', '2019-11-30T16:23:40Z',
                '2019-11-30T16:23:38Z')

    release = get_updates(test, True, 2)

    if release is not None:
        for re in release:
            li = re.to_dict()
            for k in li:
                print(f"{k}: {li[k]}")


test_get_release()
