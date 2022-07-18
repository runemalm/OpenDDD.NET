FROM python:3.10-bullseye

WORKDIR /build

COPY requirements.txt requirements.txt

RUN pip3 install -r requirements.txt

# Gen code
COPY gen_code /tools/gen_code
RUN chmod +x /tools/gen_code/main.py
RUN ln -s /tools/gen_code/main.py /usr/local/bin/gen_code

# Help
COPY help /tools/help
RUN chmod +x /tools/help/main.py
RUN ln -s /tools/help/main.py /usr/local/bin/help

WORKDIR /

ENV PYTHONPATH "${PYTHONPATH}:/"

CMD ["help"]
